using Genius.Atom.Infrastructure.Logging.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Genius.Atom.Infrastructure.Logging;

public static class LoggingModule
{
    public static void Configure(IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddTransient<EventBasedLoggerProvider>();
        services.AddLogging(x => x.AddSerilog(dispose: true));

        ConfigureSerilog(configuration);
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
