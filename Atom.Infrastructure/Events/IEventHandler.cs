namespace Genius.Atom.Infrastructure.Events;

public interface IEventHandler
{
}

public interface IEventHandler<TEvent> : IEventHandler
    where TEvent: IEventMessage
{
    Task ProcessAsync(TEvent @event);
}
