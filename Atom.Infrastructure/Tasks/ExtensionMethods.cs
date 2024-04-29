
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Tasks;

public static class ExtensionMethods
{
    /// <summary>
    ///   Extension method to run the <paramref name="task"/> and ignore the result, with an exception handling, if any.
    /// </summary>
    /// <param name="task">The task.</param>
    public static void RunAndForget(this Task task)
    {
        task.NotNull().ContinueWith(HandleException, CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private static void HandleException(Task task)
    {
        task.Exception?.Flatten().Handle((Exception ex) =>
        {
            try
            {
                var logger = Module.ServiceProvider.GetService<ILogger>();
                if (logger is not null)
                {
                    logger.LogError(ex, "Error occurred while running a task.");
                }
            }
            catch {}

            return true;
        });
    }
}