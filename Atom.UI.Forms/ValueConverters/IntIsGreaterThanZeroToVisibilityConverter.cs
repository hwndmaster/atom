namespace Genius.Atom.UI.Forms;

internal sealed class IntIsGreaterThanZeroToVisibilityConverter : MarkupFuncConverterBase<int, Visibility>
{
    public IntIsGreaterThanZeroToVisibilityConverter()
        : base(
            (v) => v is int intValue && intValue != 0,
            0, 1,
            Visibility.Visible, Visibility.Collapsed)
    {
    }
}

internal sealed class InvertedIntIsGreaterThanZeroToVisibilityConverter : MarkupFuncConverterBase<int, Visibility>
{
    public InvertedIntIsGreaterThanZeroToVisibilityConverter()
        : base(
            (v) => v is int intValue && intValue != 0,
            0, 1,
            Visibility.Collapsed, Visibility.Visible)
    {
    }
}
