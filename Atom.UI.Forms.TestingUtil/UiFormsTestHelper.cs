using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Threading;
using Genius.Atom.Infrastructure;

namespace Genius.Atom.UI.Forms.TestingUtil;

public static class UiFormsTestHelper
{
    public static void RaisePropertyChanged<T>(Mock<T> container, Expression<Func<T, object>> propertyNameExpr, object? value)
        where T : class, IViewModel
    {
        var propertyName = ExpressionHelpers.GetPropertyName(propertyNameExpr);

        container.Setup(x => x.TryGetPropertyValue(propertyName, out value))
            .Returns(true);

        container.Raise(x => x.PropertyChanged += null,
            new PropertyChangedEventArgs(propertyName));
    }

    public static void SetupDispatcher()
    {
        var frame = new DispatcherFrame();
#pragma warning disable VSTHRD110 // Observe result of async calls
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => frame.Continue = false));
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
#pragma warning restore VSTHRD110 // Observe result of async calls
        Dispatcher.PushFrame(frame);
    }
}
