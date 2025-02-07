namespace Genius.Atom.Infrastructure.Entities;

public abstract class EntityBase : IEntity
{
    private Guid _id;

    internal void SetId(Guid id) => _id = id;

#pragma warning disable S2292 // Trivial properties should be auto-implemented
    // Cannot make it auto-property because the setter has to be init-only, but
    // still rewritable for testing purposes.
    public Guid Id { get => _id; init => _id = value; }
#pragma warning restore S2292 // Trivial properties should be auto-implemented
}
