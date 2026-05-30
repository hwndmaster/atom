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

await app.RunAsync().ConfigureAwait(false);
