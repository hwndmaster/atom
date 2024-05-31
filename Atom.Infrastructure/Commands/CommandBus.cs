using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Infrastructure.Commands;

public interface ICommandBus
{
    Task SendAsync(ICommandMessage command);
    Task<TResult> SendAsync<TResult>(ICommandMessageExchange<TResult> command);
}

/// <summary>
/// A simple implementation of a command bus
/// </summary>
internal sealed class CommandBus : ICommandBus
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _handlersMethodCache = new();
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CommandBus(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task SendAsync(ICommandMessage command)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        return InvokeCommandHandlerProcessAsync(command, handlerType);
    }

    public Task<TResult> SendAsync<TResult>(ICommandMessageExchange<TResult> command)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        return (Task<TResult>)InvokeCommandHandlerProcessAsync(command, handlerType);
    }

    private Task InvokeCommandHandlerProcessAsync(ICommandMessage command, Type handlerType)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetService(handlerType);
        if (service == null)
        {
            throw new NotSupportedException($"Command handler for {command.GetType().Name} is not implemented/mapped");
        }

        var commandType = command.GetType();
        var method = _handlersMethodCache.GetOrAdd(commandType, key =>
        {
            return service.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(x => x.Name == "ProcessAsync" && x.GetParameters()[0].ParameterType == key);
        });

        var result = method.Invoke(service, new object[] { command });
        if (result is Task taskResult)
        {
            return taskResult;
        }

        throw new InvalidOperationException("Command Handler process has failed due to unknown error.");
    }
}
