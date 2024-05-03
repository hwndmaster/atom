namespace Genius.Atom.UI.Forms.TestingUtil;

public sealed class TestUiDispatcher : IUiDispatcher
{
    public void Invoke(Action action)
    {
        action();
    }

    public Task InvokeAsync(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    public Task<T> InvokeAsync<T>(Func<T> func)
    {
        return Task.FromResult(func());
    }
}
