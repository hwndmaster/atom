using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media;

namespace Genius.Atom.UI.Forms.Wpf;

[ExcludeFromCodeCoverage]
public static class WpfVisualChildrenExtensions
{
    extension(DependencyObject dependencyObject)
    {
        public IEnumerable<T> FindVisualChildren<T>() where T : DependencyObject
        {
            if (dependencyObject is null)
                yield break;

            if (dependencyObject is T t)
                yield return t;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                foreach (T childOfChild in child.FindVisualChildren<T>())
                {
                    yield return childOfChild;
                }
            }
        }
    }
}

[ExcludeFromCodeCoverage]
public static class WpfVisualParentExtensions
{
    extension(UIElement element)
    {
        public T? FindVisualParent<T>() where T : UIElement
        {
            UIElement? parent = element;
            while (parent is not null)
            {
                if (parent is T correctlyTyped)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
    }
}

[ExcludeFromCodeCoverage]
public static class WpfFrameworkElementExtensions
{
    extension(FrameworkElement frameworkElement)
    {
        public IObservable<Unit> WhenLoadedOneTime()
        {
            return Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                h => frameworkElement.Loaded += h, h => frameworkElement.Loaded -= h)
                .Take(1)
                .Select(x => Unit.Default);
        }
    }
}
