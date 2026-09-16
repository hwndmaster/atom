namespace Genius.Atom.Web.Telemetry.Observability;

public sealed class AtomWebObservabilityOptions
{
    public string ApplicationName { get; set; } = string.Empty;

    public string ActivitySourceName { get; set; } = string.Empty;

    public string VersionedRouteToken { get; set; } = "{version:apiVersion}";

    public string HealthEndpointPath { get; set; } = "/health";

    public string AlivenessEndpointPath { get; set; } = "/alive";

    public bool EnableServiceDiscovery { get; set; } = true;

    public bool EnableMvcTraceBreakdown { get; set; } = true;

    public bool MapHealthEndpointsInDevelopmentOnly { get; set; } = true;

    /// <summary>
    /// Writes one summary line per HTTP request — method, target, status, duration and trace id — so a
    /// failure in the log has the calls that led up to it for context. The middleware is inserted at the
    /// very start of the pipeline via an <c>IStartupFilter</c>, so it observes the status code whatever
    /// later middleware finally writes.
    /// </summary>
    public bool EnableRequestSummaryLogging { get; set; } = true;

    /// <summary>
    /// Path prefixes excluded from request logging. Health is probed every few seconds by a supervising
    /// Aspire app host, and OTLP is the telemetry export itself; logging either drowns out everything.
    /// </summary>
    public IList<string> RequestLogIgnoredPathPrefixes { get; } = ["/health", "/alive", "/otlp"];

    /// <summary>
    /// Dumps the environment at Debug level during the startup summary, with anything whose name looks
    /// like a credential redacted.
    /// </summary>
    public bool LogEnvironmentVariablesAtDebug { get; set; } = true;

    internal void EnsureDefaults(string fallbackApplicationName)
    {
        if (string.IsNullOrWhiteSpace(ApplicationName))
        {
            ApplicationName = fallbackApplicationName;
        }

        if (string.IsNullOrWhiteSpace(ActivitySourceName))
        {
            ActivitySourceName = $"{ApplicationName}.Mvc";
        }
    }
}
