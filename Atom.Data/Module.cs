global using Genius.Atom.Infrastructure;

using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Data.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Data;

[ExcludeFromCodeCoverage]
public static class Module
{
    public static void Configure(IServiceCollection services)
    {
        services.AddSingleton<IJsonPersister, JsonPersister>();
        services.AddSingleton<TypeDiscriminators>();
        services.AddSingleton<ITypeDiscriminators>(x => x.GetRequiredService<TypeDiscriminators>());
        services.AddSingleton<ITypeDiscriminatorsInternal>(x => x.GetRequiredService<TypeDiscriminators>());
    }
}
