using Genius.Atom.Web.Telemetry.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Genius.Atom.Web.Telemetry;

public static class TelemetryModuleExtensions
{
    public static AtomWebObservabilityOptions AddAtomWebTelemetry(this WebApplicationBuilder builder,
        Action<AtomWebObservabilityOptions>? configureObservabilityOptions = null)
    {
        Guard.NotNull(builder);

        AtomWebObservabilityOptions options = new()
        {
            ApplicationName = builder.Environment.ApplicationName,
            ActivitySourceName = $"{builder.Environment.ApplicationName}.Mvc",
        };
        configureObservabilityOptions?.Invoke(options);
        options.EnsureDefaults(builder.Environment.ApplicationName);

        builder.Services.AddSingleton(options);

        builder.AddServiceDefaults(options);

        if (options.EnableMvcTraceBreakdown)
        {
            builder.Services.AddSingleton(_ => new ApiTraceBreakdownFilter(options.ActivitySourceName));
            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, ApiTraceBreakdownMvcOptionsSetup>();
        }

        if (options.EnableRequestSummaryLogging)
        {
            // Registered as a startup filter rather than left to the application to place with
            // app.UseMiddleware: that made the ordering an app-level decision, and it has to be the
            // OUTERMOST middleware to observe the status code that error-handling middleware further in
            // finally writes. A startup filter always runs before the application's own pipeline.
            builder.Services.AddSingleton<IStartupFilter, RequestSummaryLoggingStartupFilter>();
        }

        return options;
    }

    /// <summary>
    /// Logs the effective configuration once: application, version, environment, URLs, CORS origins, the
    /// OTLP endpoint and the health endpoints, plus whatever the application adds through
    /// <paramref name="configureSummary"/>. Call after the application is built and before it runs.
    /// </summary>
    /// <param name="app">The built web application.</param>
    /// <param name="configureSummary">Adds application-specific values, e.g. the database path.</param>
    /// <returns>The application, for chaining.</returns>
    public static WebApplication LogAtomStartupSummary(this WebApplication app,
        Action<StartupSummary>? configureSummary = null)
    {
        Guard.NotNull(app);

        var options = app.Services.GetRequiredService<AtomWebObservabilityOptions>();

        StartupSummary summary = new();
        configureSummary?.Invoke(summary);

        StartupSummaryLogger.Log(app, options, summary);

        return app;
    }

    private sealed class RequestSummaryLoggingStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            Guard.NotNull(next);

            return builder =>
            {
                builder.UseMiddleware<RequestSummaryLoggingMiddleware>();
                next(builder);
            };
        }
    }

    public static WebApplication MapAtomWebTelemetryEndpoints(this WebApplication app)
    {
        Guard.NotNull(app);

        var options = app.Services.GetRequiredService<AtomWebObservabilityOptions>();
        app.MapDefaultEndpoints(options);

        return app;
    }

    private sealed class ApiTraceBreakdownMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {
            Guard.NotNull(options);

            options.Filters.AddService<ApiTraceBreakdownFilter>();
        }
    }
}
