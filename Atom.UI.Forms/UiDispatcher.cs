using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;

namespace Genius.Atom.UI.Forms;

public interface IUiDispatcher
{
    /// <summary>
    ///   Executes the specified Action synchronously on the thread that the Dispatcher was created on.
    /// </summary>
    /// <param name="action">An Action delegate to invoke through the dispatcher.</param>
    /// <param name="cancellationToken">
    ///   A cancellation token that can be used to cancel the operation.
    ///   If the operation has not started, it will be aborted when the
    ///   cancellation token is canceled. If the operation has started,
    ///   the operation can cooperate with the cancellation request.
    /// </param>
    void Invoke(Action action, CancellationToken? cancellationToken = null);

    /// <summary>
    ///   Executes the specified Action asynchronously on the thread that the Dispatcher was created on.
    /// </summary>
    /// <param name="action">An Action delegate to invoke through the dispatcher.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the operation.
    ///     If the operation has not started, it will be aborted when the
    ///     cancellation token is canceled.  If the operation has started,
    ///     the operation can cooperate with the cancellation request.
    /// </param>
    /// <returns>An operation representing the queued delegate to be invoked.</returns>
    /// <SecurityNote>
    ///     Critical: This code causes arbitrary delegate to execute asynchronously, also calls critical code.
    ///     Safe: Executing the delegate asynchronously is OK because we capture the ExecutionContext.
    /// </SecurityNote>
    Task InvokeAsync(Action action, CancellationToken? cancellationToken = null);

    /// <summary>
    ///   Executes the specified Func<TResult> asynchronously on the thread that the Dispatcher was created on.
    /// </summary>
    /// <param name="action">A Func<TResult> delegate to invoke through the dispatcher.</param>
    /// <param name="cancellationToken">
    ///   A cancellation token that can be used to cancel the operation.
    ///   If the operation has not started, it will be aborted when the
    ///   cancellation token is canceled.  If the operation has started,
    ///   the operation can cooperate with the cancellation request.
    /// </param>
    /// <returns>An operation representing the queued delegate to be invoked.</returns>
    /// <SecurityNote>
    ///   Critical: This code causes arbitrary delegate to execute asynchronously, also calls critical code.
    ///   Safe: Executing the delegate asynchronously is OK because we capture the ExecutionContext inside the DispatcherOperation.
    /// </SecurityNote>
    Task<T> InvokeAsync<T>(Func<T> func, CancellationToken? cancellationToken = null);
}

[ExcludeFromCodeCoverage]
internal sealed class UiDispatcher : IUiDispatcher
{
    private readonly Application _application;

    public UiDispatcher(Application application)
    {
        _application = application.NotNull();
    }

    public void Invoke(Action action, CancellationToken? cancellationToken = null)
    {
        _application.Dispatcher.Invoke(action, DispatcherPriority.Normal, cancellationToken ?? CancellationToken.None);
    }

    public Task InvokeAsync(Action action, CancellationToken? cancellationToken = null)
    {
        var invocation = _application.Dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken ?? CancellationToken.None);
        return invocation.Task;
    }

    public Task<T> InvokeAsync<T>(Func<T> func, CancellationToken? cancellationToken = null)
    {
        var invocation = _application.Dispatcher.InvokeAsync(func, DispatcherPriority.Normal, cancellationToken ?? CancellationToken.None);
        return invocation.Task;
    }
}