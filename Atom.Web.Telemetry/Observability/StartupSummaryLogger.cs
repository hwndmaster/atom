using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Web.Telemetry.Observability;

/// <summary>
/// Logs the effective configuration once, at startup. When a service misbehaves on a server the first
/// question is always "which settings is it actually running with" — on a container that is otherwise
/// unanswerable, because the values come from a mix of appsettings, environment variables and defaults.
/// Runs once per process, so it costs nothing at request time.
/// </summary>
internal static class StartupSummaryLogger
{
    /// <summary>
    /// Substrings that mark an environment variable as carrying a credential. Dumping those verbatim
    /// would copy live credentials into the log file and into the Aspire dashboard.
    /// </summary>
    private static readonly string[] SecretKeyMarkers =
        ["TOKEN", "SECRET", "PASSWORD", "PWD", "APIKEY", "API_KEY", "CONNECTIONSTRING", "CREDENTIAL"];

    public static void Log(WebApplication app, AtomWebObservabilityOptions options, StartupSummary summary)
    {
        Guard.NotNull(app);
        Guard.NotNull(options);
        Guard.NotNull(summary);

        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        var configuration = app.Configuration;

        logger.LogInformation(
            "Starting {Application} {Version} | Environment: {Environment} | ContentRoot: {ContentRoot} | Urls: {Urls}",
            app.Environment.ApplicationName,
            GetInformationalVersion(),
            app.Environment.EnvironmentName,
            app.Environment.ContentRootPath,
            configuration["ASPNETCORE_URLS"] ?? "(host defaults)");

        var corsOrigins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        logger.LogInformation(
            "CORS origins: {CorsOrigins} | OTLP endpoint: {OtlpEndpoint} | Health endpoints: {HealthEndpoints}",
            corsOrigins.Length > 0 ? string.Join(", ", corsOrigins) : "(none configured)",
            string.IsNullOrWhiteSpace(otlpEndpoint) ? "(none - telemetry is discarded)" : otlpEndpoint,
            options.MapHealthEndpointsInDevelopmentOnly && !app.Environment.IsDevelopment()
                ? "(not mapped outside Development)"
                : $"{options.HealthEndpointPath}, {options.AlivenessEndpointPath}");

        if (summary.Values.Count > 0)
        {
            logger.LogInformation("{ApplicationSummary}",
                string.Join(" | ", summary.Values.Select(x => $"{x.Key}: {x.Value}")));
        }

        LogEnvironmentVariables(logger, options);
    }

    /// <summary>
    /// Dumps the environment at Debug, with anything that looks like a credential redacted. Off unless
    /// Debug logging is enabled, so it costs nothing normally.
    /// </summary>
    private static void LogEnvironmentVariables(ILogger logger, AtomWebObservabilityOptions options)
    {
        if (!options.LogEnvironmentVariablesAtDebug || !logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        StringBuilder envVars = new();
        foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
        {
            var key = env.Key.ToString() ?? string.Empty;
            envVars.AppendLine(System.Globalization.CultureInfo.InvariantCulture,
                $"{key}={(IsSecret(key) ? "***redacted***" : env.Value)}");
        }

        logger.LogDebug("Environment variables:\n{EnvVars}", envVars);
    }

    private static bool IsSecret(string key)
    {
        foreach (var marker in SecretKeyMarkers)
        {
            if (key.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetInformationalVersion()
        => Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            ?? "unknown";
}
