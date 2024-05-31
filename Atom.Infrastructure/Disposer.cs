using System.Diagnostics.CodeAnalysis;

namespace Genius.Atom.Infrastructure;

public sealed class Disposer : IDisposable
{
    private readonly List<DisposableAction> _disposeActions = [];
    private volatile bool _isDisposed;

    public bool IsDisposed => _isDisposed;

    [return: NotNullIfNotNull(nameof(disposable))]
    public T? Add<T>(T? disposable)
        where T : IDisposable
    {
        if (disposable is null)
            return disposable;
        Add(new DisposableAction(disposable.Dispose));
        return disposable;
    }

    public void Add(Action action)
    {
        _disposeActions.Add(new DisposableAction(action));
    }

    public void Add(DisposableAction disposableAction)
    {
        if (_isDisposed)
        {
            disposableAction?.Dispose();
            return;
        }
        _disposeActions.Add(disposableAction);
    }

    public void Dispose()
    {
        _isDisposed = true;
        DisposableAction[] disposables;
        lock (_disposeActions)
        {
            disposables = _disposeActions.ToArray();
            _disposeActions.Clear();
        }
        for (var i = disposables.Length - 1; i >= 0; i--)
        {
            disposables[i]?.Dispose();
        }
    }
}
