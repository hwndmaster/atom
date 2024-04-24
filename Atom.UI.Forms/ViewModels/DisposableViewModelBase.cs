namespace Genius.Atom.UI.Forms;

/// <summary>
///   An abstract class for disposable view models.
/// </summary>
public abstract class DisposableViewModelBase : ViewModelBase, IDisposable
{
    protected Disposer Disposer { get; } = new();

    public void Dispose()
    {
        Disposer.Dispose();
    }
}
