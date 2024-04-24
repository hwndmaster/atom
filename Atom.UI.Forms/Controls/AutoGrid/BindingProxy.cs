namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

internal sealed class BindingProxy : Freezable
{
    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data),
        typeof(object), typeof(BindingProxy));

    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy()
        {
            Data = Data
        };
    }

    public object Data
    {
        get { return GetValue(DataProperty); }
        set { SetValue(DataProperty, value); }
    }
}
