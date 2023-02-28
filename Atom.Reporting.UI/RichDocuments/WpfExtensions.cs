namespace Genius.Atom.Reporting.UI.RichDocuments;

public static class WpfExtensions
{
    public static System.Windows.Media.Color ToWpfColor(this System.Drawing.Color color)
    {
        return System.Windows.Media.Color.FromArgb(
            color.A,
            color.R,
            color.G,
            color.B
        );
    }

    public static System.Windows.Thickness ToWpfThickness(this Reporting.RichDocuments.Thickness thickness)
    {
        return new System.Windows.Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
    }

    public static System.Windows.TextAlignment ToWpfTextAlignment(this Reporting.RichDocuments.RichTextAlignment textAlignment)
    {
        return textAlignment switch
        {
            Reporting.RichDocuments.RichTextAlignment.Left => System.Windows.TextAlignment.Left,
            Reporting.RichDocuments.RichTextAlignment.Right => System.Windows.TextAlignment.Right,
            Reporting.RichDocuments.RichTextAlignment.Center => System.Windows.TextAlignment.Center,
            Reporting.RichDocuments.RichTextAlignment.Justify => System.Windows.TextAlignment.Justify,
            _ => throw new NotSupportedException("TextAlignment not supported: " + textAlignment),
        };
    }
}
