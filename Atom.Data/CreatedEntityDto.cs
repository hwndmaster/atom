namespace Genius.Atom.Data;

public sealed record CreatedEntityDto<TKey, TReference>(TReference EntityId, DateTimeOffset LastModified)
    where TKey : notnull
    where TReference : IReference<TKey, TReference>;
