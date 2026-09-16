using Genius.Atom.Web.OpenApi;
using Genius.Atom.Web.Telemetry;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Environment.ContentRootPath = Path.Combine(AppContext.BaseDirectory);
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "Logs"));

Genius.Atom.Infrastructure.Module.Configure(builder.Services, builder.Configuration);
Genius.Atom.Web.Module.Configure(builder, new ApiVersion(1, 0));

builder.AddAtomWebTelemetry(options =>
{
    options.ApplicationName = "Genius.Atom.Web.Demo";
    options.ActivitySourceName = "Genius.Atom.Web.Demo.Mvc";
});

builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer<DateTimeOffsetSchemaTransformer>();
    options.AddOperationTransformer<ReferenceParameterTransformer>();
});

var app = builder.Build();

Genius.Atom.Infrastructure.Module.Initialize(app.Services);
Genius.Atom.Web.Module.Initialize(app);

app.MapAtomWebTelemetryEndpoints();
app.MapControllers();

// One line per request is written automatically (AtomWebObservabilityOptions.EnableRequestSummaryLogging);
// this adds the configuration summary, with whatever the application wants to report about itself.
app.LogAtomStartupSummary(summary => summary
    .AddFile("Database", Path.Combine(app.Environment.ContentRootPath, "Data", "Demo.db"))
    .Add("Demo setting", "example value"));

await app.RunAsync().ConfigureAwait(false);
