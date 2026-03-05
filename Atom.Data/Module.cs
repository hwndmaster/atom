global using Genius.Atom.Infrastructure;

using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Data.IdHandlers;
using Genius.Atom.Data.JsonPersistence;
using Genius.Atom.Data.TypeVersioning;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Data;

[ExcludeFromCodeCoverage]
public static class Module
{
    private static IServiceProvider? _serviceProvider;
    internal static IServiceProvider ServiceProvider
        => _serviceProvider ?? throw new InvalidOperationException("Call Genius.Atom.Data.Module.Initialize(serviceProvider) in your application initialization.");

    public static void Configure(IServiceCollection services)
    {
        // Id handlers
        services.AddTransient<IIdHandler<Guid>, GuidIdHandler>();
        services.AddTransient<IIdHandler<int>, IntIdHandler>();

        // Json persistence
        services.AddSingleton<IJsonPersister, JsonPersister>();

        // Type versioning
        services.AddSingleton<TypeDiscriminators>();
        services.AddSingleton<ITypeDiscriminators>(x => x.GetRequiredService<TypeDiscriminators>());
        services.AddSingleton<ITypeDiscriminatorsInternal>(x => x.GetRequiredService<TypeDiscriminators>());
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider.NotNull();
    }
}
