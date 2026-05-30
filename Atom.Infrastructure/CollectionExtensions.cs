namespace Genius.Atom.Infrastructure;

public static class CollectionExtensions
{
    extension<T>(ICollection<T> collection)
    {
        public void ReplaceItems(IEnumerable<T> items)
        {
            Guard.NotNull(items);

            collection.Clear();
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
