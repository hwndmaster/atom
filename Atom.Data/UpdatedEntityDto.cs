namespace Genius.Atom.Data;

public sealed record UpdatedEntityDto<TKey, TReference>(TReference EntityId, DateTimeOffset LastModified)
    where TKey : notnull
    where TReference : IReference<TKey, TReference>;
