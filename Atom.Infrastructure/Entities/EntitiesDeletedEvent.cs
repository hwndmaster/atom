using Genius.Atom.Infrastructure.Events;

namespace Genius.Atom.Infrastructure.Entities;

public sealed class EntitiesDeletedEvent : IEventMessage
{
    public EntitiesDeletedEvent(string entityType, IEnumerable<Guid> entityIds)
    {
        Entities = entityIds.Select(id => (id, entityType)).ToArray();
        EntityIds = entityIds.ToHashSet();
    }

    public EntitiesDeletedEvent(IEnumerable<(Guid EntityId, string EntityType)> entities)
    {
        Entities = entities.ToArray();
        EntityIds = Entities.Select(x => x.EntityId).ToHashSet();
    }

    public ISet<Guid> EntityIds { get; }
    public ICollection<(Guid EntityId, string EntityType)> Entities { get; }
}
