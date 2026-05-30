using Genius.Atom.Web.Telemetry.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Genius.Atom.Web.Telemetry;

public static class ServiceDefaultsExtensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder, AtomWebObservabilityOptions options)
        where TBuilder : IHostApplicationBuilder
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.ConfigureOpenTelemetry(options);
        builder.AddDefaultHealthChecks();

        if (options.EnableServiceDiscovery)
        {
            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                http.AddStandardResilienceHandler();
                http.AddServiceDiscovery();
            });
        }

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder, AtomWebObservabilityOptions options)
        where TBuilder : IHostApplicationBuilder
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddSource(options.ApplicationName, options.ActivitySourceName)
                    .AddProcessor(new ResolvedRouteSpanNameProcessor(options.VersionedRouteToken))
                    .AddAspNetCoreInstrumentation(configuration =>
                    {
                        var healthEndpointPath = NormalizeRoutePath(options.HealthEndpointPath);
                        var alivenessEndpointPath = NormalizeRoutePath(options.AlivenessEndpointPath);

                        configuration.Filter = context =>
                            !context.Request.Path.StartsWithSegments(healthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(alivenessEndpointPath);
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();
            });

        AddOpenTelemetryExporters(builder);

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        Guard.NotNull(builder);

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app, AtomWebObservabilityOptions options)
    {
        Guard.NotNull(app);
        Guard.NotNull(options);

        if (options.MapHealthEndpointsInDevelopmentOnly && !app.Environment.IsDevelopment())
        {
            return app;
        }

        var healthEndpointPath = NormalizeRoutePath(options.HealthEndpointPath);
        var alivenessEndpointPath = NormalizeRoutePath(options.AlivenessEndpointPath);

        app.MapHealthChecks(healthEndpointPath);

        app.MapHealthChecks(alivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live")
        });

        return app;
    }

    private static void AddOpenTelemetryExporters<TBuilder>(TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        Guard.NotNull(builder);

        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }
    }

    private static string NormalizeRoutePath(string path)
    {
        Guard.NotNullOrWhitespace(path);

        return path.StartsWith('/') ? path : $"/{path}";
    }
}
