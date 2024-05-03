using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Threading;

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
    private readonly JoinableTaskContext _joinableTaskContext;
    private readonly JoinableTaskFactory _joinableTaskFactory;

    public UiDispatcher()
    {
        _joinableTaskContext = new JoinableTaskContext();
        _joinableTaskFactory = new JoinableTaskFactory(_joinableTaskContext);
    }

    public void Invoke(Action action)
    {
        _joinableTaskFactory.Run(async () =>
        {
            await _joinableTaskFactory.SwitchToMainThreadAsync();
            action();
        });
    }

    public async Task InvokeAsync(Action action)
    {
        await _joinableTaskFactory.RunAsync(async () =>
        {
            await _joinableTaskFactory.SwitchToMainThreadAsync();
            action();
        });
    }

    public async Task<T> InvokeAsync<T>(Func<T> func)
    {
        return await _joinableTaskFactory.RunAsync(async () =>
        {
            await _joinableTaskFactory.SwitchToMainThreadAsync();
            return func();
        });
    }

    public void Dispose()
    {
        _joinableTaskContext.Dispose();
    }
}