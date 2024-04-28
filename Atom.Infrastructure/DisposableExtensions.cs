namespace Genius.Atom.Infrastructure;

public static class DisposableExtensions
{
    /// <summary>
    ///   Ensures the provided disposable is disposed with the specified <see cref="Disposer"/>.
    /// </summary>
    /// <typeparam name="T">The type of the disposable.</typeparam>
    /// <param name="item">The disposable we are going to want to be disposed by the <see cref="Disposer"/>.</param>
    /// <param name="disposer">The <see cref="Disposer"/> to which item will be added..</param>
    /// <returns>The disposable</returns>
    public static T DisposeWith<T>(this T item, Disposer disposer) where T : IDisposable
    {
        Guard.NotNull(disposer);

        disposer.Add(item);
        return item;
    }
}
