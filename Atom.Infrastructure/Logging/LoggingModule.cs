using Genius.Atom.Infrastructure.Logging.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Genius.Atom.Infrastructure.Logging;

public static class LoggingModule
{
    /// <summary>
    /// Full type names of the logger providers the .NET generic host registers by default (see
    /// <c>HostingHostBuilderExtensions.AddDefaultBuiltInProviders</c>).
    /// <para>
    /// Matched by name rather than by <c>typeof</c> on purpose: Atom.Infrastructure references only
    /// Microsoft.Extensions.Logging.Abstractions, and taking a dependency on each concrete provider
    /// package merely to name its type would push those packages onto every consuming application.
    /// </para>
    /// </summary>
    private static readonly string[] HostDefaultLoggerProviderTypeNames =
    [
        "Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider",
        "Microsoft.Extensions.Logging.Debug.DebugLoggerProvider",
        "Microsoft.Extensions.Logging.EventSource.EventSourceLoggerProvider",
        "Microsoft.Extensions.Logging.EventLog.EventLogLoggerProvider",
    ];

    /// <summary>
    /// Registers Atom's logging: the event-based provider and, unless told otherwise, Serilog.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration to read the Serilog section from.</param>
    /// <param name="includeSerilog">Whether Serilog is wired up at all. The testing utilities pass false.</param>
    /// <param name="mode">
    /// Whether Serilog joins the host's logger providers or replaces them. Ignored when
    /// <paramref name="includeSerilog"/> is false — removing the host's providers with nothing taking
    /// their place would leave the application with no logging output at all.
    /// </param>
    public static void Configure(IServiceCollection services, IConfiguration? configuration = null,
        bool includeSerilog = true, AtomLoggingMode mode = AtomLoggingMode.Additive)
    {
        Guard.NotNull(services);

        services.AddTransient<EventBasedLoggerProvider>();

        if (!includeSerilog)
        {
            services.AddLogging();
            return;
        }

        if (mode == AtomLoggingMode.ReplaceHostDefaults)
        {
            RemoveHostDefaultLoggerProviders(services);
        }

        services.AddLogging(x => x.AddSerilog(dispose: true));
        ConfigureSerilog(configuration);
    }

    /// <summary>
    /// Removes the logger providers the .NET generic host registers by default — Console, Debug,
    /// EventSource and, on Windows, EventLog — and leaves every other registered provider alone.
    /// <para>
    /// Safe to call at any point during service registration: it matches on what is in the collection
    /// rather than on when it was put there, unlike <c>ILoggingBuilder.ClearProviders()</c>, which
    /// discards whatever happens to have been registered so far.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to remove the providers from.</param>
    public static void RemoveHostDefaultLoggerProviders(IServiceCollection services)
    {
        Guard.NotNull(services);

        // Backwards: removing shifts every later index down.
        for (var i = services.Count - 1; i >= 0; i--)
        {
            var descriptor = services[i];
            if (descriptor.ServiceType != typeof(ILoggerProvider))
            {
                continue;
            }

            var implementationTypeName = descriptor.ImplementationType?.FullName;
            if (implementationTypeName is not null
                && Array.IndexOf(HostDefaultLoggerProviderTypeNames, implementationTypeName) >= 0)
            {
                services.RemoveAt(i);
            }
        }
    }

    public static void ConfigureSerilog(IConfiguration? configuration = null)
    {
        var logConfig = new LoggerConfiguration()
            .Enrich.WithThreadId()
            .Enrich.WithComputed("SourceContextName", "Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)");

        if (configuration != null)
        {
            logConfig = logConfig.ReadFrom.Configuration(configuration);
        }

        Log.Logger = logConfig.CreateLogger();
    }
}
