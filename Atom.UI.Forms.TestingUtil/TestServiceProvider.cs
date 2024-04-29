using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.TestingUtil;

public sealed class TestServiceProvider : IServiceProvider, IDisposable
{
    private readonly ServiceCollection _serviceCollection;
    private ServiceProvider? _serviceProvider;

    public TestServiceProvider()
    {
        _serviceCollection = new ServiceCollection();
    }

    public void AddSingleton<TService, TImplementation>(bool isTestImplementation = false)
        where TService : class
        where TImplementation : class, TService
    {
        CheckIntegrity();

        if (isTestImplementation)
        {
            _serviceCollection.AddSingleton<TImplementation>();
            _serviceCollection.AddSingleton<TService>((sp) => sp.GetService<TImplementation>());
        }
        else
        {
            _serviceCollection.AddSingleton<TService, TImplementation>();
        }
    }

    public void RegisterInstance<T>(T instance)
        where T : class
    {
        CheckIntegrity();

        _serviceCollection.AddSingleton<T>(instance);
    }

    private void CheckIntegrity()
    {
        if (_serviceProvider is not null)
        {
            throw new InvalidOperationException("No registrations can be done once any service has been resolved.");
        }
    }

    public object? GetService(Type serviceType)
    {
        EnsureServiceProvider();

        return _serviceProvider.GetService(serviceType);
    }

    [MemberNotNull(nameof(_serviceProvider))]
    private void EnsureServiceProvider()
    {
        _serviceProvider ??= _serviceCollection.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
