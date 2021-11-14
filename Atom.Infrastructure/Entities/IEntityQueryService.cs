using System;
using System.Collections.Generic;

namespace Genius.Atom.Infrastructure.Entities;

public interface IEntityQueryService<TEntity>
    where TEntity: EntityBase
{
    IEnumerable<TEntity> GetAll();
    TEntity? FindById(Guid entityId);
}
