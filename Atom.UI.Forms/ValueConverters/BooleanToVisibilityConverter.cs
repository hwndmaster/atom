namespace Genius.Atom.UI.Forms;

public sealed class BooleanToVisibilityConverter : MarkupBooleanConverterBase<Visibility>
{
    public BooleanToVisibilityConverter()
        : base(Visibility.Visible, Visibility.Collapsed) { }
}
