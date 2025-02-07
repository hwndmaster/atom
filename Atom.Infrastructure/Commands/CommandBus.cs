using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Genius.Atom.Infrastructure.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Commands;

public interface ICommandBus
{
    Task SendAsync(ICommandMessage command);
    Task<TResult> SendAsync<TResult>(ICommandMessageExchange<TResult> command);
}

/// <summary>
/// A simple implementation of a command bus
/// </summary>
internal sealed class CommandBus : ICommandBus, IDisposable
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _handlersMethodCache = new();
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ISynchronousScheduler _synchronousScheduler;
    private readonly ILogger<CommandBus> _logger;

    public CommandBus(IServiceScopeFactory serviceScopeFactory, ISynchronousScheduler synchronousScheduler, ILogger<CommandBus> logger)
    {
        _serviceScopeFactory = serviceScopeFactory.NotNull();
        _synchronousScheduler = synchronousScheduler.NotNull();
        _logger = logger.NotNull();
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
        var commandHandler = scope.ServiceProvider.GetService(handlerType)
            ?? throw new NotSupportedException($"Command handler for {command.GetType().Name} is not implemented/mapped");

        MethodInfo method = FindProcessMethod(command, commandHandler);

        using AutoResetEvent autoResetEvent = new(false);
        Task? commandInvocationResult = null;
        _synchronousScheduler.Schedule(async () =>
        {
            try
            {
                if (IsDisposed)
                {
                    return;
                }

                var result = method.Invoke(commandHandler, [command]);
                if (result is Task taskResult)
                {
                    commandInvocationResult = taskResult;
                    await taskResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Command execution failed. HandlerType: '{0}', Command:\r\n{1}",
                    handlerType.FullName,
                    JsonSerializer.Serialize<object>(command, new JsonSerializerOptions() { MaxDepth = 10 }));
            }
            finally
            {
                autoResetEvent.Set();
            }
        });

        autoResetEvent.WaitOne();

        if (commandInvocationResult is not null)
        {
            return commandInvocationResult;
        }

        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(CommandBus));
        }

        throw new InvalidOperationException("Command Handler process has failed due to an unexpected error. Check logs for details.");
    }

    private static MethodInfo FindProcessMethod(ICommandMessage command, object commandHandler)
    {
        var commandType = command.GetType();
        return _handlersMethodCache.GetOrAdd(commandType, key =>
        {
            return commandHandler.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(x => x.Name == "ProcessAsync" && x.GetParameters()[0].ParameterType == key);
        });
    }

    public void Dispose()
    {
        IsDisposed = true;
    }

    public bool IsDisposed { get; private set; }
}
