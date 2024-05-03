namespace Genius.Atom.Infrastructure.Events;

public interface IEventHandler
{
}

public interface IEventHandler<in TEvent> : IEventHandler
    where TEvent: IEventMessage
{
    Task ProcessAsync(TEvent @event);
}
