using System.Reactive;
using System.Reactive.Linq;

namespace Genius.Atom.Infrastructure.Events;

public interface IEventBus
{
    void Publish(IEventMessage @event);
    IObservable<T> WhenFired<T>()
        where T : IEventMessage;
    IObservable<Unit> WhenFired<T1, T2>()
        where T1 : IEventMessage
        where T2 : IEventMessage;
    IObservable<Unit> WhenFired<T1, T2, T3>()
        where T1 : IEventMessage
        where T2 : IEventMessage
        where T3 : IEventMessage;
}

internal sealed class EventBus : IEventBus
{
    private event EventHandler<EventPublishedArgs>? EventAdded;
    private readonly IObservable<EventPublishedArgs> _mainObservable;

    public EventBus()
    {
        _mainObservable = Observable.FromEventPattern<EventPublishedArgs>(
            x => this.EventAdded += x,
            x => this.EventAdded -= x)
            .Select(x => x.EventArgs);
    }

    public void Publish(IEventMessage message)
    {
        Guard.NotNull(message);

        EventAdded?.Invoke(this, new EventPublishedArgs(message));
    }

    public IObservable<T> WhenFired<T>()
        where T : IEventMessage
    {
        return _mainObservable
            .Where(x => x.Event is T)
            .Select(x => (T)x.Event);
    }

    public IObservable<Unit> WhenFired<T1, T2>()
        where T1 : IEventMessage
        where T2 : IEventMessage
    {
        return WhenFired<T1>().Select(_ => Unit.Default)
            .Merge(WhenFired<T2>().Select(_ => Unit.Default));
    }

    public IObservable<Unit> WhenFired<T1, T2, T3>()
        where T1 : IEventMessage
        where T2 : IEventMessage
        where T3 : IEventMessage
    {
        return WhenFired<T1, T2>()
            .Merge(WhenFired<T3>().Select(_ => Unit.Default));
    }
}
