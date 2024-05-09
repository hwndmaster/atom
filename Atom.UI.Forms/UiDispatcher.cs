using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Infrastructure.Threading;

namespace Genius.Atom.UI.Forms;

public interface IUiDispatcher
{
    /// <summary>
    ///   Executes the specified action synchronously on the thread that the Dispatcher was created on.
    /// </summary>
    /// <param name="action">An action delegate to invoke through the dispatcher.</param>
    void Invoke(Action action);

    /// <summary>
    ///   Executes the specified action asynchronously on the thread that the Dispatcher was created on.
    /// </summary>
    /// <param name="action">An action delegate to invoke through the dispatcher.</param>
    /// <returns>An operation representing the queued delegate to be invoked.</returns>
    Task InvokeAsync(Action action);

    /// <summary>
    ///   Executes the specified Func<TResult> asynchronously on the thread that the Dispatcher was created on.
    /// </summary>
    /// <param name="action">A Func<TResult> delegate to invoke through the dispatcher.</param>
    /// <returns>An operation representing the queued delegate to be invoked.</returns>
    Task<T> InvokeAsync<T>(Func<T> func);
}

[ExcludeFromCodeCoverage]
internal sealed class UiDispatcher : IUiDispatcher, IDisposable
{
    private readonly JoinableTaskHelper _joinableTask = new();

    public void Invoke(Action action)
    {
        _joinableTask.Factory.Run(async () =>
        {
            await _joinableTask.Factory.SwitchToMainThreadAsync();
            action();
        });
    }

    public async Task InvokeAsync(Action action)
    {
        await _joinableTask.Factory.RunAsync(async () =>
        {
            await _joinableTask.Factory.SwitchToMainThreadAsync();
            action();
        });
    }

    public async Task<T> InvokeAsync<T>(Func<T> func)
    {
        return await _joinableTask.Factory.RunAsync(async () =>
        {
            await _joinableTask.Factory.SwitchToMainThreadAsync();
            return func();
        });
    }

    public void Dispose()
    {
        _joinableTask.Dispose();
    }
}
