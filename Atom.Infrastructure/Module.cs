using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Infrastructure.Commands;
using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.Io;
using Genius.Atom.Infrastructure.Net;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Infrastructure;

[ExcludeFromCodeCoverage]
public static class Module
{
    public static void Configure(IServiceCollection services)
    {
        services.AddSingleton<ICommandBus, CommandBus>();
        services.AddSingleton<IDateTime, SystemDateTime>();
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<ITrickyHttpClient, TrickyHttpClient>();
    }
}
