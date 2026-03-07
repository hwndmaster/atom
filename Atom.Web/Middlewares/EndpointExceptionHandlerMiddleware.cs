using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Web.Middlewares;

/// <summary>
/// Component that catches unhandled exception thrown later in the pipeline and returns it as a 409 Conflict response.
/// </summary>
internal sealed class InvalidOperationExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InvalidOperationExceptionHandlerMiddleware> _logger;

    public InvalidOperationExceptionHandlerMiddleware(RequestDelegate next, ILogger<InvalidOperationExceptionHandlerMiddleware> logger)
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
                : "A server error occurred. Check the logs for more details.";
            string payload = JsonSerializer.Serialize(message);
            await context.Response.WriteAsync(payload).ConfigureAwait(false);
        }
    }
}
