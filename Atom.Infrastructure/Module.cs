using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Infrastructure.Commands;
using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.Io;
using Genius.Atom.Infrastructure.Net;
using Genius.Atom.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Infrastructure;

[ExcludeFromCodeCoverage]
public static class Module
{
    public static void Configure(IServiceCollection services)
    {
        services.AddSingleton<ICommandBus, CommandBus>();
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IJsonPersister, JsonPersister>();
        services.AddSingleton<ITrickyHttpClient, TrickyHttpClient>();
    }
}
