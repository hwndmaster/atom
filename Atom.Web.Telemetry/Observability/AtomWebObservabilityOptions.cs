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
