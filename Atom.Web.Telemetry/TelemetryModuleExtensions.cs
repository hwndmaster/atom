using Genius.Atom.Web.Telemetry.Observability;
using Microsoft.AspNetCore.Builder;
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

        return options;
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
