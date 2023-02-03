global using Genius.Atom.Infrastructure;

using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Data.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Data;

[ExcludeFromCodeCoverage]
public static class Module
{
    private static IServiceProvider? _serviceProvider;
    internal static IServiceProvider ServiceProvider
        => _serviceProvider ?? throw new ArgumentNullException("Call Genius.Atom.Data.Module.Initialize(serviceProvider) in your application initialization.");

    public static void Configure(IServiceCollection services)
    {
        services.AddSingleton<IJsonPersister, JsonPersister>();
        services.AddSingleton<TypeDiscriminators>();
        services.AddSingleton<ITypeDiscriminators>(x => x.GetRequiredService<TypeDiscriminators>());
        services.AddSingleton<ITypeDiscriminatorsInternal>(x => x.GetRequiredService<TypeDiscriminators>());
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider.NotNull();
    }
}
