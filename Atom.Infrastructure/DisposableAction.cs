namespace Genius.Atom.Infrastructure;

public sealed class DisposableAction : IDisposable
{
    private readonly Action _disposeAction;

    public DisposableAction(Action disposeAction)
    {
        Guard.NotNull(disposeAction);

        _disposeAction = disposeAction;
    }

    public void Dispose()
    {
        _disposeAction();
    }
}
