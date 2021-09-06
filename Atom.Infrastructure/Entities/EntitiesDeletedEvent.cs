using System;
using System.Collections.Generic;
using System.Linq;
using Genius.Atom.Infrastructure.Events;

namespace Genius.Atom.Infrastructure.Entities
{
    public sealed class EntitiesDeletedEvent : IEventMessage
    {
        public EntitiesDeletedEvent(Type entityType, IEnumerable<Guid> entityIds)
        {
            Entities = entityIds.Select(id => (id, entityType)).ToArray();
            EntityIds = entityIds.ToHashSet();
        }

        public EntitiesDeletedEvent(IEnumerable<(Guid EntityId, Type EntityType)> entities)
        {
            Entities = entities.ToArray();
            EntityIds = Entities.Select(x => x.EntityId).ToHashSet();
        }

        public HashSet<Guid> EntityIds { get; }
        public ICollection<(Guid EntityId, Type EntityType)> Entities { get; }
    }
}
