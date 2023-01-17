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
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }
}
