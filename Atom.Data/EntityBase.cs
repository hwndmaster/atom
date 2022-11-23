namespace Genius.Atom.Infrastructure.Entities;

public abstract class EntityBase : IEntity
{
    private Guid _id;

    internal void SetId(Guid id) => _id = id;

    public Guid Id { get => _id; init => _id = value; }
}
