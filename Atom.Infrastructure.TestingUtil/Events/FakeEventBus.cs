using System.Collections.Immutable;
using System.Reactive;
using Genius.Atom.Infrastructure.Events;

namespace Genius.Atom.Infrastructure.TestingUtil.Events;

public sealed class FakeEventBus : IEventBus
{
    private readonly List<IEventMessage> _publishedEvents = new();
    private readonly EventBus _origin = new();

    public void Publish(IEventMessage eventMessage)
    {
        _publishedEvents.Add(eventMessage);
        _origin.Publish(eventMessage);
    }

    public IObservable<T> WhenFired<T>() where T : IEventMessage
    {
        return _origin.WhenFired<T>();
    }

    public IObservable<Unit> WhenFired<T1, T2>()
        where T1 : IEventMessage
        where T2 : IEventMessage
    {
        return _origin.WhenFired<T1, T2>();
    }

    public IObservable<Unit> WhenFired<T1, T2, T3>()
        where T1 : IEventMessage
        where T2 : IEventMessage
        where T3 : IEventMessage
    {
        return _origin.WhenFired<T1, T2, T3>();
    }

    public void AssertSingleEvent<T>(Func<T, bool> condition)
        where T : IEventMessage
    {
        Guard.NotNull(condition);

        var @event = GetSingleEvent<T>();
        Assert.True(condition(@event));
    }

    public void AssertSingleEvent<T>(params Action<T>[] actionToAssert)
        where T : IEventMessage
    {
        Guard.NotNull(actionToAssert);

        var @event = GetSingleEvent<T>();
        foreach (var action in actionToAssert)
        {
            action(@event);
        }
    }

    public void AssertNoEventOfType<T>()
        where T : IEventMessage
    {
        Assert.False(_publishedEvents.OfType<T>().Any());
    }

    public void AssertNoEventsButOfType<T>()
        where T : IEventMessage
    {
        Assert.False(_publishedEvents.Any(x => x is not T));
    }

    public T GetSingleEvent<T>()
        where T : IEventMessage
    {
        var matchedEvents = _publishedEvents.OfType<T>().ToList();
        Assert.Single(matchedEvents);
        return matchedEvents[0];
    }

    public ImmutableArray<IEventMessage> PublishedEvents => _publishedEvents.ToImmutableArray();
}
