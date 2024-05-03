namespace Genius.Atom.Infrastructure.Events;

internal sealed class EventPublishedEventArgs : EventArgs
{
    public EventPublishedEventArgs(IEventMessage @event)
    {
        Guard.NotNull(@event);

        Event = @event;
    }

    public IEventMessage Event { get; }
}
