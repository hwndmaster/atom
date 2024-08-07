namespace Genius.Atom.Infrastructure;

public static class CollectionExtensions
{
    public static void ReplaceItems<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        Guard.NotNull(items);

        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
