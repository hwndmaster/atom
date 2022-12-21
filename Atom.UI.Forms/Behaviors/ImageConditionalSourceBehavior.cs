using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms.Behaviors;

public sealed class ImageConditionalSourceBehavior : Behavior<ListBox>
{
    public static readonly DependencyProperty WhenTrueProperty = DependencyProperty.RegisterAttached(
        "WhenTrue",
        typeof(object),
        typeof(ImageConditionalSourceBehavior),
        new PropertyMetadata(OnWhenTrueChanged));

    public static readonly DependencyProperty WhenFalseProperty = DependencyProperty.RegisterAttached(
        "WhenFalse",
        typeof(object),
        typeof(ImageConditionalSourceBehavior),
        new PropertyMetadata(OnWhenFalseChanged));

    public static object GetWhenTrue(DependencyObject element) => element.GetValue(WhenTrueProperty);
    public static void SetWhenTrue(DependencyObject element, object value) => element.SetValue(WhenTrueProperty, value);

    public static object GetWhenFalse(DependencyObject element) => element.GetValue(WhenFalseProperty);
    public static void SetWhenFalse(DependencyObject element, object value) => element.SetValue(WhenFalseProperty, value);

    private static void OnWhenTrueChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        var image = (Image)element;
        SetupStyleTrigger(image, true);
    }

    private static void OnWhenFalseChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        var image = (Image)element;
        SetupStyleTrigger(image, false);
    }

    private static void SetupStyleTrigger(Image image, bool whenValue)
    {
        var style = new Style(typeof(Image), image.Style);
        var trigger = new DataTrigger
        {
            Binding = new Binding("."),
            Value = whenValue,
        };

        trigger.Setters.Add(new Setter
        {
            Property = Image.SourceProperty,
            Value = whenValue ? GetWhenTrue(image) : GetWhenFalse(image)
        });

        style.Triggers.Add(trigger);

        image.Style = style;
    }
}
