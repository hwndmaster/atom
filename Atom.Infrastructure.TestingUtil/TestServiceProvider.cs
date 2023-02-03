using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Infrastructure.TestingUtil;

public class TestServiceProvider : IServiceProvider
{
    private readonly ServiceCollection _serviceCollection = new();
    private IServiceProvider? _serviceProvider;

    public void RegisterSingleton<T>()
    {
        _serviceProvider = null;
        _serviceCollection.AddSingleton(typeof(T));
    }

    public void RegisterSingleton<TService, TImplementation>()
    {
        _serviceProvider = null;
        _serviceCollection.AddSingleton(typeof(TService), typeof(TImplementation));
    }

    public void RegisterInstance<T>(T instance)
    {
        Guard.NotNull(instance);

        _serviceProvider = null;
        _serviceCollection.AddSingleton(typeof(T), instance);
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
}
