using System.ComponentModel;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms;

/// <summary>
///   Behavior to clean up all attached behaviors which implement <see cref="IDisposable"/>.
/// </summary>
public class TakeCareOfDisposablesBehavior : Behavior<FrameworkElement>
{
    /// <summary>
    ///   Attaches this behavior to the specified <paramref name="associatedObject"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="associatedObject"/>.</typeparam>
    /// <param name="associatedObject">The framework element to which the behavior needs to be attached.</param>
    public static void AttachMe<T>(T associatedObject)
        where T : FrameworkElement
    {
        associatedObject.Loaded += OnLoaded;

        void OnLoaded(object? sender, EventArgs e)
        {
            associatedObject.Loaded -= OnLoaded;

            var behaviors = Interaction.GetBehaviors(associatedObject);
            if (!behaviors.OfType<TakeCareOfDisposablesBehavior>().Any())
                behaviors.Add(new TakeCareOfDisposablesBehavior());
        }
    }

    /// <summary>
    ///   Called after the behavior is attached to an AssociatedObject.
    /// </summary>
    protected override void OnAttached()
    {
        AssociatedObject.Loaded += UserControlLoadedHandler;

        base.OnAttached();
    }

    /// <summary>
    ///   Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
    /// </summary>
    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= UserControlLoadedHandler;
        var window = Window.GetWindow(AssociatedObject);
        if (window is not null)
            window.Closing -= WindowClosingHandler;

        base.OnDetaching();
    }

    private void UserControlLoadedHandler(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(AssociatedObject);
        if (window is null)
        {
            throw new Exception($"The UserControl {AssociatedObject.GetType().Name} is not contained within a Window. The TakeCareOfDisposablesBehavior cannot be used.");
        }

        window.Closing += WindowClosingHandler;
    }

    private void WindowClosingHandler(object? sender, CancelEventArgs e)
    {
        var behaviors = Interaction.GetBehaviors(AssociatedObject);
        foreach (var behavior in behaviors)
        {
            if (behavior is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
