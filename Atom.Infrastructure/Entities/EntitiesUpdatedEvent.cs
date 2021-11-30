using Genius.Atom.Infrastructure.Events;

namespace Genius.Atom.Infrastructure.Entities;

public sealed class EntitiesUpdatedEvent : IEventMessage
{
    public EntitiesUpdatedEvent(IEnumerable<Guid> entities)
    {
        Entities = entities.ToList();
    }

    public IReadOnlyCollection<Guid> Entities { get; }
}
