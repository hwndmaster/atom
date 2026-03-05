namespace Genius.Atom.Data;

public interface IReference<out TKey>
{
    TKey Id { get; }
}

public interface IReference<TKey, out TReference> : IReference<TKey>
    where TKey : notnull
    where TReference : IReference<TKey, TReference>
{
    bool IsDefault() => Id.Equals(default!);

    abstract static TReference Create(TKey id);
}
