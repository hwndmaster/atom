using Genius.Atom.Infrastructure;

namespace Genius.Atom.UI.Forms.TestingUtil;

public sealed class TestUiDispatcher : IUiDispatcher
{
    public void Invoke(Action action)
    {
        Guard.NotNull(action);

        action();
    }

    public Task InvokeAsync(Action action)
    {
        Guard.NotNull(action);

        action();
        return Task.CompletedTask;
    }

    public Task InvokeAsync(Func<Task> asyncFunc)
    {
        Guard.NotNull(asyncFunc);

        return asyncFunc();
    }
}
