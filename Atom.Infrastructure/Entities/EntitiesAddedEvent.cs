using Genius.Atom.Infrastructure.Events;

namespace Genius.Atom.Infrastructure.Entities;

public sealed class EntitiesAddedEvent : IEventMessage
{
    public EntitiesAddedEvent(IEnumerable<Guid> entities)
    {
        Guard.NotNull(entities);

        Entities = entities.ToList();
    }

    public IReadOnlyCollection<Guid> Entities { get; }
}
