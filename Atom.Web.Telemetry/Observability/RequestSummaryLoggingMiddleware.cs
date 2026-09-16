using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Web.Telemetry.Observability;

/// <summary>
/// Writes exactly one summary line per HTTP request, so that a failure in the log has the calls that led
/// up to it for context. Deliberately one line and not a start/stop pair: the per-request cost is a
/// <see cref="Stopwatch"/> and a single structured log write.
/// <para>
/// Goes through <see cref="ILogger{TCategoryName}"/> rather than Serilog's own request logging so the
/// line reaches both sinks — the Serilog file sink <em>and</em> the OpenTelemetry provider feeding the
/// Aspire dashboard. Serilog's <c>UseSerilogRequestLogging</c> writes straight to the static
/// <c>Log.Logger</c> and would never show up in the dashboard.
/// </para>
/// </summary>
public sealed class RequestSummaryLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestSummaryLoggingMiddleware> _logger;
    private readonly string[] _ignoredPathPrefixes;

    public RequestSummaryLoggingMiddleware(RequestDelegate next,
        ILogger<RequestSummaryLoggingMiddleware> logger, AtomWebObservabilityOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Guard.NotNull(options);

        _ignoredPathPrefixes = [.. options.RequestLogIgnoredPathPrefixes];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Guard.NotNull(context);

        if (IsIgnored(context.Request.Path))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // The exception itself is logged by whatever handles it further in; reaching this catch means
            // it escaped even that, so record the request shape and rethrow.
            LogRequest(context, LogLevel.Error, stopwatch, ex);
            throw;
        }

        stopwatch.Stop();
        LogRequest(context, GetLogLevel(context.Response.StatusCode), stopwatch, exception: null);
    }

    private void LogRequest(HttpContext context, LogLevel level, Stopwatch stopwatch, Exception? exception)
    {
        if (!_logger.IsEnabled(level))
        {
            return;
        }

        // Property names avoid RequestPath / RequestId / TraceId / SpanId / ParentId deliberately.
        // ASP.NET Core pushes those as a logging scope around every request, and Serilog resolves the
        // ambient scope property ahead of the one in this template — naming the argument RequestPath
        // silently replaced the value below with the scope's, which is the path *without* the query.
        // RequestTraceId ties this line to the distributed trace of the same request in the dashboard,
        // and to the browser spans a SPA exports for the user action that triggered it.
        _logger.Log(
            level,
            exception,
            "HTTP {HttpMethod} {RequestTarget} responded {ResponseStatusCode} in {ElapsedMs:0.0} ms. TraceId: {RequestTraceId}",
            context.Request.Method,
            BuildPath(context.Request),
            context.Response.StatusCode,
            stopwatch.Elapsed.TotalMilliseconds,
            Activity.Current?.TraceId.ToString() ?? "n/a");
    }

    /// <summary>
    /// Includes the query string: for these APIs it carries entity ids and filters, which is most of what
    /// makes a logged request reproducible.
    /// </summary>
    private static string BuildPath(HttpRequest request)
        => request.QueryString.HasValue
            ? string.Concat(request.Path.Value, request.QueryString.Value)
            : request.Path.Value ?? "/";

    private static LogLevel GetLogLevel(int statusCode)
        => statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information,
        };

    private bool IsIgnored(PathString path)
    {
        foreach (var prefix in _ignoredPathPrefixes)
        {
            if (path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
