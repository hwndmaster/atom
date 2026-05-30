using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Genius.Atom.Web.Telemetry.Observability;

public sealed class ApiTraceBreakdownFilter : IAsyncActionFilter, IAsyncResultFilter, IDisposable
{
    private readonly ActivitySource _activitySource;

    public ApiTraceBreakdownFilter(string activitySourceName)
    {
        Guard.NotNullOrWhitespace(activitySourceName);

        _activitySource = new ActivitySource(activitySourceName);
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        using var activity = _activitySource.StartActivity("mvc.action.execute", ActivityKind.Internal);
        PopulateRouteTags(activity, context);
        activity?.SetTag("app.mvc.action.arguments.count", context.ActionArguments.Count);

        Stopwatch stopwatch = Stopwatch.StartNew();
        ActionExecutedContext executedContext = await next().ConfigureAwait(false);
        stopwatch.Stop();

        activity?.SetTag("app.mvc.action.duration_ms", stopwatch.Elapsed.TotalMilliseconds);
        activity?.SetTag("app.mvc.model_state.is_valid", context.ModelState.IsValid);

        if (executedContext.Exception is not null && !executedContext.ExceptionHandled)
        {
            activity?.SetStatus(ActivityStatusCode.Error, executedContext.Exception.Message);
            return;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        using var activity = _activitySource.StartActivity("mvc.result.execute", ActivityKind.Internal);
        PopulateRouteTags(activity, context);

        if (context.Result is ObjectResult objectResult && objectResult.Value is not null)
        {
            int? itemCount = TryGetCollectionCount(objectResult.Value);
            if (itemCount.HasValue)
            {
                activity?.SetTag("app.payload.items", itemCount.Value);
            }
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        ResultExecutedContext resultExecutedContext = await next().ConfigureAwait(false);
        stopwatch.Stop();

        HttpResponse response = context.HttpContext.Response;
        activity?.SetTag("app.mvc.result.duration_ms", stopwatch.Elapsed.TotalMilliseconds);
        activity?.SetTag("http.response.status_code", response.StatusCode);
        activity?.SetTag("http.response.content_type", response.ContentType);

        if (response.ContentLength.HasValue)
        {
            activity?.SetTag("http.response.body.size", response.ContentLength.Value);
        }

        if (resultExecutedContext.Exception is not null && !resultExecutedContext.ExceptionHandled)
        {
            activity?.SetStatus(ActivityStatusCode.Error, resultExecutedContext.Exception.Message);
            return;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    private static void PopulateRouteTags(Activity? activity, FilterContext context)
    {
        if (activity is null)
        {
            return;
        }

        if (context.RouteData.Values.TryGetValue("controller", out object? controller))
        {
            activity.SetTag("aspnetcore.mvc.controller", controller?.ToString());
        }

        if (context.RouteData.Values.TryGetValue("action", out object? action))
        {
            activity.SetTag("aspnetcore.mvc.action", action?.ToString());
        }

        activity.SetTag("aspnetcore.mvc.display_name", context.ActionDescriptor.DisplayName);
    }

    private static int? TryGetCollectionCount(object value)
    {
        if (value is string)
        {
            return null;
        }

        if (value is ICollection collection)
        {
            return collection.Count;
        }

        if (value is IEnumerable)
        {
            Type? readOnlyCollectionInterface = value.GetType()
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));

            if (readOnlyCollectionInterface is not null)
            {
                return (int?)readOnlyCollectionInterface.GetProperty("Count")?.GetValue(value);
            }
        }

        return null;
    }

    public void Dispose()
    {
        _activitySource.Dispose();
    }
}
