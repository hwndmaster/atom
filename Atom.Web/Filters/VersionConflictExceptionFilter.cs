using Genius.Atom.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Web.Filters;

/// <summary>
/// Translates the optimistic-concurrency <see cref="EntityVersionConflictException"/> thrown by the
/// Genius.Atom repositories into a descriptive HTTP 409 (Conflict) response and logs enriched context,
/// so the conflicting entity type and id are available to clients and diagnostics without decoding the
/// raw exception message. Registered globally for all controllers by <see cref="Module.Configure"/>.
/// </summary>
internal sealed class VersionConflictExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<VersionConflictExceptionFilter> _logger;

    public VersionConflictExceptionFilter(ILogger<VersionConflictExceptionFilter> logger)
    {
        _logger = logger;
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        Guard.NotNull(context);

        if (context.Exception is not EntityVersionConflictException exception)
        {
            return Task.CompletedTask;
        }

        _logger.LogWarning(
            exception,
            "Optimistic concurrency conflict while updating {EntityName} #{EntityId}. The submitted "
            + "version ({AttemptedLastModified}) no longer matches the stored one ({StoredLastModified}) "
            + "— the entity was changed elsewhere since it was loaded, or the request was submitted twice.",
            exception.EntityName,
            exception.Id,
            exception.AttemptedLastModified,
            exception.StoredLastModified);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Version conflict",
            Detail = $"The {exception.EntityName} (#{exception.Id}) was changed by someone else since it "
                + "was loaded. Reload the latest version and try again.",
        };
        problemDetails.Extensions["entityType"] = exception.EntityName;
        problemDetails.Extensions["entityId"] = exception.Id;

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status409Conflict,
        };
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
