using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms;

public sealed class ImageConditionalSourceBehavior : Behavior<Image>
{
    public static readonly DependencyProperty FlagValueProperty = DependencyProperty.Register(
        nameof(FlagValue),
        typeof(bool),
        typeof(ImageConditionalSourceBehavior),
        new PropertyMetadata(null));

    public static readonly DependencyProperty WhenTrueProperty = DependencyProperty.Register(
        nameof(WhenTrue),
        typeof(object),
        typeof(ImageConditionalSourceBehavior),
        new PropertyMetadata(null));

    public static readonly DependencyProperty WhenFalseProperty = DependencyProperty.Register(
        nameof(WhenFalse),
        typeof(object),
        typeof(ImageConditionalSourceBehavior),
        new PropertyMetadata(null));

    protected override void OnAttached()
    {
        base.OnAttached();

        var binding = BindingOperations.GetBindingExpression(this, FlagValueProperty);

        var style = new Style(typeof(Image), AssociatedObject.Style);

        var trueValueTrigger = CreateTrigger(binding.ParentBinding, true);
        style.Triggers.Add(trueValueTrigger);

        var falseValueTrigger = CreateTrigger(binding.ParentBinding, false);
        style.Triggers.Add(falseValueTrigger);

        AssociatedObject.Style = style;
    }

    private DataTrigger CreateTrigger(Binding binding, bool whenValue)
    {
        var trigger = new DataTrigger
        {
            Binding = binding,
            Value = whenValue,
        };

        trigger.Setters.Add(new Setter
        {
            Property = Image.SourceProperty,
            Value = whenValue ? WhenTrue : WhenFalse
        });

        return trigger;
    }

    public bool FlagValue
    {
        get { return (bool)GetValue(FlagValueProperty); }
        set { SetValue(FlagValueProperty, value); }
    }

    public object? WhenTrue
    {
        get { return GetValue(WhenTrueProperty); }
        set { SetValue(WhenTrueProperty, value); }
    }

    public object? WhenFalse
    {
        get { return GetValue(WhenFalseProperty); }
        set { SetValue(WhenFalseProperty, value); }
    }
}
