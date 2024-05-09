namespace Genius.Atom.UI.Forms;

/// <summary>
///   An abstract class for disposable view models.
/// </summary>
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
public abstract class DisposableViewModelBase : ViewModelBase, IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
{
    protected Disposer Disposer { get; } = new();

    public void Dispose()
    {
        Disposer.Dispose();
    }
}
