using System.Diagnostics;
using OpenTelemetry;

namespace Genius.Atom.Web.Telemetry.Observability;

public sealed class ResolvedRouteSpanNameProcessor : BaseProcessor<Activity>
{
    private readonly string _versionedRouteToken;

    public ResolvedRouteSpanNameProcessor(string versionedRouteToken)
    {
        Guard.NotNullOrWhitespace(versionedRouteToken);

        _versionedRouteToken = versionedRouteToken;
    }

    public override void OnEnd(Activity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        if (activity.Kind != ActivityKind.Server)
        {
            return;
        }

        var route = GetStringTag(activity, "http.route");
        if (string.IsNullOrWhiteSpace(route) || !route.Contains(_versionedRouteToken, StringComparison.Ordinal))
        {
            return;
        }

        var path = GetStringTag(activity, "url.path") ?? GetStringTag(activity, "http.target");
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var queryStart = path.IndexOf('?', StringComparison.Ordinal);
        if (queryStart >= 0)
        {
            path = path[..queryStart];
        }

        var resolvedRoute = path.TrimStart('/');
        if (string.IsNullOrWhiteSpace(resolvedRoute))
        {
            return;
        }

        var method = GetStringTag(activity, "http.request.method") ?? GetStringTag(activity, "http.method");
        activity.DisplayName = string.IsNullOrWhiteSpace(method)
            ? resolvedRoute
            : $"{method} {resolvedRoute}";
        activity.SetTag("app.http.route.resolved", resolvedRoute);
    }

    private static string? GetStringTag(Activity activity, string key)
    {
        foreach (var tag in activity.TagObjects)
        {
            if (tag.Key == key)
            {
                return tag.Value?.ToString();
            }
        }

        return null;
    }
}
