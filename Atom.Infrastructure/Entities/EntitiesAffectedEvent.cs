using Genius.Atom.Infrastructure.Events;

namespace Genius.Atom.Infrastructure.Entities;

public sealed class EntitiesAffectedEvent : IEventMessage
{
    public EntitiesAffectedEvent(IEnumerable<(Guid, Type)>? entitiesAdded,
        IEnumerable<(Guid, Type)>? entitiesUpdated,
        IEnumerable<(Guid, Type)>? entitiesDeleted)
    {
        Added = entitiesAdded is null ? new Dictionary<Guid, Type>()
            : entitiesAdded.ToDictionary(x => x.Item1, x => x.Item2);
        Updated = entitiesUpdated is null ? new Dictionary<Guid, Type>()
            : entitiesUpdated.ToDictionary(x => x.Item1, x => x.Item2);
        Deleted = entitiesDeleted is null ? new Dictionary<Guid, Type>()
            : entitiesDeleted.ToDictionary(x => x.Item1, x => x.Item2);
        TypesAffected = new HashSet<Type>();

        foreach (var entity in Added)
            TypesAffected.Add(entity.Value);
        foreach (var entity in Updated)
            TypesAffected.Add(entity.Value);
        foreach (var entity in Deleted)
            TypesAffected.Add(entity.Value);
    }

    public EntitiesAffectedEvent(Type entityType, IEnumerable<Guid>? entityAddedIds,
        IEnumerable<Guid>? entityUpdatedIds,
        IEnumerable<Guid>? entityDeletedIds)
    {
        Guard.NotNull(entityType);

        Added = entityAddedIds is null ? new Dictionary<Guid, Type>()
            : entityAddedIds.ToDictionary(x => x, _ => entityType);
        Updated = entityUpdatedIds is null ? new Dictionary<Guid, Type>()
            : entityUpdatedIds.ToDictionary(x => x, _ => entityType);
        Deleted = entityDeletedIds is null ? new Dictionary<Guid, Type>()
            : entityDeletedIds.ToDictionary(x => x, _ => entityType);
        TypesAffected = new HashSet<Type>(new[] { entityType });
    }

    public EntitiesAffectedEvent(Type entityType, EntityAffectedEventType eventType, params Guid[] entityIds)
    {
        Guard.NotNull(entityType);
        Guard.NotNull(entityIds);

        var added = new Dictionary<Guid, Type>();
        var updated = new Dictionary<Guid, Type>();
        var deleted = new Dictionary<Guid, Type>();
        var operatingDict = eventType switch {
            EntityAffectedEventType.Added => added,
            EntityAffectedEventType.Updated => updated,
            EntityAffectedEventType.Deleted => deleted,
            _ => throw new ArgumentException($"Unknown event type '{eventType}'")
        };
        TypesAffected = new HashSet<Type>(new[] { entityType });

        foreach (var entityId in entityIds)
        {
            operatingDict.Add(entityId, entityType);
        }

        Added = added;
        Updated = updated;
        Deleted = deleted;
    }

    public IReadOnlyDictionary<Guid, Type> Added { get; }
    public IReadOnlyDictionary<Guid, Type> Updated { get; }
    public IReadOnlyDictionary<Guid, Type> Deleted { get; }
    public ISet<Type> TypesAffected { get; }
}
