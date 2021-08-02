using System;

namespace Genius.Atom.Infrastructure.Events
{
    internal sealed class EventPublishedArgs : EventArgs
    {
        public EventPublishedArgs(IEventMessage @event)
        {
            Event = @event;
        }

        public IEventMessage Event { get; }
    }
}