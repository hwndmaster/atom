using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Infrastructure.Commands;
using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.Io;
using Genius.Atom.Infrastructure.Logging;
using Genius.Atom.Infrastructure.Logging.Events;
using Genius.Atom.Infrastructure.Net;
using Genius.Atom.Infrastructure.Tasks;
using Genius.Atom.Infrastructure.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure;

[ExcludeFromCodeCoverage]
public static class Module
{
    private static IServiceProvider? _serviceProvider;
    internal static IServiceProvider ServiceProvider
        => _serviceProvider ?? throw new InvalidOperationException("Call Genius.Atom.Infrastructure.Module.Initialize(serviceProvider) in your application initialization.");

    public static void Configure(IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddTransient(typeof(Lazy<>), typeof(Lazier<>));
        services.AddSingleton(typeof(IFactory<>), typeof(ServiceFactory<>));
        services.AddSingleton<IDateTime, SystemDateTime>();

        // Commands
        services.AddSingleton<ICommandBus, CommandBus>();

        // Events
        services.AddSingleton<IEventBus, EventBus>();

        // Logging
        LoggingModule.Configure(services, configuration);

        // Net
        services.AddSingleton<ITrickyHttpClient, TrickyHttpClient>();

        // IO
        services.AddSingleton<IFileService, FileService>();
        services.AddTransient<IFileSystemWatcherFactory, FileSystemWatcherFactory>();

        // Tasks
        services.AddTransient<ISynchronousScheduler, SynchronousScheduler>();

        // Threading
        services.AddTransient<JoinableTaskHelper>();
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider.NotNull();

        serviceProvider
            .GetService<ILoggerFactory>()
            .NotNull()
            .AddProvider(serviceProvider.GetRequiredService<EventBasedLoggerProvider>());
    }
}
