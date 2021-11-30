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
    }
}
