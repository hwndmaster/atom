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

    /// <summary>
    /// Registers Atom's infrastructure services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration, used among other things to read the Serilog section.</param>
    /// <param name="configureOptions">
    /// Opt-in behaviour. Every default matches how Atom behaved before the options existed, so omitting
    /// this changes nothing for an application that already works.
    /// </param>
    public static void Configure(IServiceCollection services, IConfiguration? configuration = null,
        Action<AtomInfrastructureOptions>? configureOptions = null)
    {
        Guard.NotNull(services);

        AtomInfrastructureOptions options = new();
        configureOptions?.Invoke(options);

        services.AddTransient(typeof(Lazy<>), typeof(Lazier<>));
        services.AddSingleton(typeof(IFactory<>), typeof(ServiceFactory<>));
        services.AddSingleton<IDateTime, SystemDateTime>();

        // Commands
        services.AddSingleton<ICommandBus, CommandBus>();

        // Events
        services.AddSingleton<IEventBus, EventBus>();

        // Logging
        LoggingModule.Configure(services, configuration, mode: options.LoggingMode);

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
