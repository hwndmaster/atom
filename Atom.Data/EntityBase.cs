using System.ComponentModel.DataAnnotations;

namespace Genius.Atom.Data;

public abstract record EntityBase<TKey, TReference> : IEntity<TKey, TReference>
    where TKey : notnull
    where TReference : IReference<TKey, TReference>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private TReference _id;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal void SetId(TReference id) => _id = id;

    // Cannot make it auto-property because the setter has to be init-only, but
    // still rewritable for testing purposes.
    [Key]
#pragma warning disable S2292 // Trivial properties should be auto-implemented
#pragma warning disable RCS1085 // Use auto-implemented property
    public TReference Id { get => _id; init => _id = value; }
#pragma warning restore RCS1085 // Use auto-implemented property
#pragma warning restore S2292 // Trivial properties should be auto-implemented

    public DateTimeOffset DateCreated { get; init; }
    public DateTimeOffset LastModified { get; init; }
}
