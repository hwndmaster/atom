using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;

namespace Genius.Atom.UI.Forms.Wpf;

[ExcludeFromCodeCoverage]
public static class WpfExtensions
{
    public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject dependencyObject) where T : DependencyObject
    {
        if (dependencyObject is null)
            yield break;

        if (dependencyObject is T t)
            yield return t;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
            foreach (T childOfChild in FindVisualChildren<T>(child))
            {
                yield return childOfChild;
            }
        }

        /*if (dependencyObject is ContentPresenter contentPresenter)
        {
            if (contentPresenter.Content is DependencyObject contentDependencyObject)
            {
                foreach (T childOfChild in FindVisualChildren<T>(contentDependencyObject))
                {
                    yield return childOfChild;
                }
            }
        }*/
    }

    public static T? FindVisualParent<T>(this UIElement element) where T : UIElement
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
