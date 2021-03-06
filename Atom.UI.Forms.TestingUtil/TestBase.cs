using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Windows.Threading;
using AutoFixture;
using Genius.Atom.Infrastructure;
using Genius.Atom.Infrastructure.Events;
using Moq;

namespace Genius.Atom.UI.Forms.TestingUtil;

public abstract class TestBase
{
    protected TestBase()
    {
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth: 2));

        SetupDispatcher();
    }

    protected Subject<T> CreateEventSubject<T>()
        where T : IEventMessage
    {
        Subject<T> subject = new();
        EventBusMock.Setup(x => x.WhenFired<T>())
            .Returns(subject);
        return subject;
    }

    protected void RaisePropertyChanged<T>(Mock<T> container, Expression<Func<T, object>> propertyNameExpr, object? value)
        where T : class, IViewModel
    {
        var propertyName = ExpressionHelpers.GetPropertyName(propertyNameExpr);

        container.Setup(x => x.TryGetPropertyValue(propertyName, out value))
            .Returns(true);

        container.Raise(x => x.PropertyChanged += null,
            new PropertyChangedEventArgs(propertyName));
    }

    private static void SetupDispatcher()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    protected Fixture Fixture { get; } = new();

    private readonly Lazy<Mock<IEventBus>> _eventBusMock = new(() => new Mock<IEventBus>());
    protected Mock<IEventBus> EventBusMock => _eventBusMock.Value;
}
