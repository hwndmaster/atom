using System;

namespace Genius.Atom.Infrastructure.Entities;

public abstract class EntityBase : IEntity
{
    public Guid Id { get; set; }
}
