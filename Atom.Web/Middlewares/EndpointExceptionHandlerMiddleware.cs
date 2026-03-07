using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Web.Middlewares;

/// <summary>
/// Component that catches unhandled exception thrown later in the pipeline and returns it as a 409 Conflict response.
/// </summary>
internal sealed class EndpointExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EndpointExceptionHandlerMiddleware> _logger;

    public EndpointExceptionHandlerMiddleware(RequestDelegate next, ILogger<EndpointExceptionHandlerMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Guard.NotNull(context);

        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex) when (!context.Response.HasStarted) // If the response started we cannot modify status code safely; rethrow.
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";

            _logger.LogError(ex, "Unhandled exception in endpoint.");

            // Use detailed message only for InvalidOperationException, otherwise generic server error.
            string message = ex is InvalidOperationException
                ? ex.Message
                : "A server error occurred, check with administrator.";
            string payload = JsonSerializer.Serialize(message);
            await context.Response.WriteAsync(payload).ConfigureAwait(false);
        }
    }
}
