namespace Genius.Atom.Infrastructure;

public static class DisposableExtensions
{
    extension<T>(T item)
        where T : IDisposable
    {
        /// <summary>
        ///   Ensures the provided disposable is disposed with the specified <see cref="Disposer"/>.
        /// </summary>
        /// <param name="disposer">The <see cref="Disposer"/> to which item will be added..</param>
        /// <returns>The disposable</returns>
        public T DisposeWith(Disposer disposer)
        {
            Guard.NotNull(disposer);

            disposer.Add(item);
            return item;
        }
    }
}
