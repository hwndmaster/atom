namespace Genius.Atom.Data;

/// <summary>
/// Interface defining the primary identifier of an entity.
/// </summary>
public interface IPrimaryId<TKey, TReference>
    where TKey : notnull
    where TReference : IReference<TKey, TReference>
{
    TReference Id { get; }
}

/// <summary>
/// Convenience interface for entities with an <see cref="int"/>-based primary identifier.
/// </summary>
public interface IPrimaryInt32Id<TReference> : IPrimaryId<int, TReference>
    where TReference : IReference<int, TReference>;
