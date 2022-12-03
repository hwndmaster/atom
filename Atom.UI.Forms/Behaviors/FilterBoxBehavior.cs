using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms.Behaviors;

public class FilterBoxBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        AssociatedObject.KeyUp += OnKeyUp;

        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.KeyUp -= OnKeyUp;

        base.OnDetaching();
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Escape)
        {
            if (e.Key == Key.Escape)
            {
                AssociatedObject.Text = string.Empty;
            }

            var bindingExpr = BindingOperations.GetBindingExpression(AssociatedObject, TextBox.TextProperty);
            bindingExpr?.UpdateSource();
        }
    }
}
