using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Infrastructure.TestingUtil;

public sealed class TestServiceProvider : IServiceProvider, IDisposable
{
    private readonly ServiceCollection _serviceCollection = new();
    private ServiceProvider? _serviceProvider;

    public void AddSingleton<TServiceAndImplementation>(bool isTestImplementation = false)
        where TServiceAndImplementation : class
    {
        AddSingleton<TServiceAndImplementation, TServiceAndImplementation>(isTestImplementation);
    }

    public void AddSingleton<TService, TImplementation>(bool isTestImplementation = false)
        where TService : class
        where TImplementation : class, TService
    {
        CheckIntegrity();

        if (isTestImplementation)
        {
            _serviceCollection.AddSingleton<TImplementation>();
            _serviceCollection.AddSingleton<TService>((sp) => sp.GetRequiredService<TImplementation>());
        }
        else
        {
            _serviceCollection.AddSingleton<TService, TImplementation>();
        }
    }

    public void RegisterInstance<T>(T instance)
        where T : class
    {
        Guard.NotNull(instance);
        CheckIntegrity();

        _serviceCollection.AddSingleton<T>(instance);
    }

    public object? GetService(Type serviceType)
    {
        EnsureServiceProvider();

        return _serviceProvider.GetService(serviceType);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [MemberNotNull(nameof(_serviceProvider))]
    private void EnsureServiceProvider()
    {
        _serviceProvider ??= _serviceCollection.BuildServiceProvider();
    }

    private void CheckIntegrity()
    {
        if (_serviceProvider is not null)
        {
            throw new InvalidOperationException("No registrations can be done once any service has been resolved.");
        }
    }
}
