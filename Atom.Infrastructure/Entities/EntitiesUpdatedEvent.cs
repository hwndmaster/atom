using System;
using System.Collections.Generic;
using System.Linq;
using Genius.Atom.Infrastructure.Events;

namespace Genius.Atom.Infrastructure.Entities
{
    public sealed class EntitiesUpdatedEvent : IEventMessage
    {
        public EntitiesUpdatedEvent(IEnumerable<EntityBase> entities)
        {
            Entities = entities.ToDictionary(x => x.Id);
        }

        public Dictionary<Guid, EntityBase> Entities { get; }
    }
}
