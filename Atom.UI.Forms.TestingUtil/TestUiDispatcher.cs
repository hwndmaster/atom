namespace Genius.Atom.UI.Forms.TestingUtil;

public sealed class TestUiDispatcher : IUiDispatcher
{
    public Task BeginInvoke(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    public void Invoke(Action action, CancellationToken? cancellationToken = null)
    {
        action();
    }

    public Task InvokeAsync(Action action, CancellationToken? cancellationToken = null)
    {
        action();
        return Task.CompletedTask;
    }

    public Task<T> InvokeAsync<T>(Func<T> func, CancellationToken? cancellationToken = null)
    {
        return Task.FromResult(func());
    }
}
