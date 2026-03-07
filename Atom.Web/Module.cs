using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Data.JsonConverters;
using Genius.Atom.Web.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Genius.Atom.Web;

[ExcludeFromCodeCoverage]
public static class Module
{
    public static void Configure(WebApplicationBuilder builder, Microsoft.AspNetCore.Mvc.ApiVersion defaultApiVersion)
    {
        Guard.NotNull(builder);

        builder.Services.AddOpenApi();

        // Add API Versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = defaultApiVersion;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });
        builder.Services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        builder.Services.Configure<JsonOptions>(options => JsonSetup.SetupJsonOptions(options.SerializerOptions));
    }

    public static void Initialize(WebApplication app)
    {
        Guard.NotNull(app);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseMiddleware<InvalidOperationExceptionHandlerMiddleware>();
    }
}
