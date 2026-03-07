using Genius.Atom.Infrastructure.Logging.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Genius.Atom.Infrastructure.Logging;

public static class LoggingModule
{
    public static void Configure(IServiceCollection services, IConfiguration? configuration = null, bool includeSerilog = true)
    {
        services.AddTransient<EventBasedLoggerProvider>();

        if (includeSerilog)
        {
            services.AddLogging(x => x.AddSerilog(dispose: true));
            ConfigureSerilog(configuration);
        }
        else
        {
            services.AddLogging();
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
