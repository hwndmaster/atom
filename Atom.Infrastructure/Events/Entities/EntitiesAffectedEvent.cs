namespace Genius.Atom.Infrastructure.Events.Entities;

public sealed class EntitiesAffectedEvent<TEntityKey> : IEventMessage
    where TEntityKey : notnull
{
    public EntitiesAffectedEvent(IEnumerable<(TEntityKey, Type)>? entitiesAdded,
        IEnumerable<(TEntityKey, Type)>? entitiesUpdated,
        IEnumerable<(TEntityKey, Type)>? entitiesDeleted)
    {
        Added = entitiesAdded is null ? []
            : entitiesAdded.ToDictionary(x => x.Item1, x => x.Item2);
        Updated = entitiesUpdated is null ? []
            : entitiesUpdated.ToDictionary(x => x.Item1, x => x.Item2);
        Deleted = entitiesDeleted is null ? []
            : entitiesDeleted.ToDictionary(x => x.Item1, x => x.Item2);
        TypesAffected = new HashSet<Type>();

        foreach (var entity in Added)
            TypesAffected.Add(entity.Value);
        foreach (var entity in Updated)
            TypesAffected.Add(entity.Value);
        foreach (var entity in Deleted)
            TypesAffected.Add(entity.Value);
    }

    public EntitiesAffectedEvent(Type entityType, IEnumerable<TEntityKey>? entityAddedIds,
        IEnumerable<TEntityKey>? entityUpdatedIds,
        IEnumerable<TEntityKey>? entityDeletedIds)
    {
        Guard.NotNull(entityType);

        Added = entityAddedIds is null ? []
            : entityAddedIds.ToDictionary(x => x, _ => entityType);
        Updated = entityUpdatedIds is null ? []
            : entityUpdatedIds.ToDictionary(x => x, _ => entityType);
        Deleted = entityDeletedIds is null ? []
            : entityDeletedIds.ToDictionary(x => x, _ => entityType);
        TypesAffected = new HashSet<Type>(new[] { entityType });
    }

    public EntitiesAffectedEvent(Type entityType, EntityAffectedEventType eventType, params TEntityKey[] entityIds)
    {
        Guard.NotNull(entityType);
        Guard.NotNull(entityIds);

        var added = new Dictionary<TEntityKey, Type>();
        var updated = new Dictionary<TEntityKey, Type>();
        var deleted = new Dictionary<TEntityKey, Type>();
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

    public IReadOnlyDictionary<TEntityKey, Type> Added { get; }
    public IReadOnlyDictionary<TEntityKey, Type> Updated { get; }
    public IReadOnlyDictionary<TEntityKey, Type> Deleted { get; }
    public ISet<Type> TypesAffected { get; }
}
