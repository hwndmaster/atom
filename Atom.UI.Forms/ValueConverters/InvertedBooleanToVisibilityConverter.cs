namespace Genius.Atom.UI.Forms;

public sealed class InvertedBooleanToVisibilityConverter : MarkupBooleanConverterBase<Visibility>
{
    public InvertedBooleanToVisibilityConverter()
        : base(Visibility.Collapsed, Visibility.Visible) { }
}
