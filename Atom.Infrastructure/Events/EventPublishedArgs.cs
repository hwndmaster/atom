namespace Genius.Atom.Infrastructure.Events;

internal sealed class EventPublishedArgs : EventArgs
{
    public EventPublishedArgs(IEventMessage @event)
    {
        Guard.NotNull(@event);

        Event = @event;
    }

    public IEventMessage Event { get; }
}
