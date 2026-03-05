namespace Genius.Atom.Data;

public interface IEntity<TKey, TReference> : IPrimaryId<TKey, TReference>, ITimeStamped
    where TKey : notnull
    where TReference : IReference<TKey, TReference>
{
    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    DateTimeOffset DateCreated { get; }
}
