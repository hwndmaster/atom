namespace Genius.Atom.Reporting.UI.RichDocuments;

public static class ColorWpfExtensions
{
    extension(System.Drawing.Color color)
    {
        public System.Windows.Media.Color ToWpfColor()
        {
            return System.Windows.Media.Color.FromArgb(
                color.A,
                color.R,
                color.G,
                color.B
            );
        }
    }
}

public static class ThicknessWpfExtensions
{
    extension(Reporting.RichDocuments.Thickness thickness)
    {
        public System.Windows.Thickness ToWpfThickness()
        {
            return new System.Windows.Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
        }
    }
}

public static class TextAlignmentWpfExtensions
{
    extension(Reporting.RichDocuments.RichTextAlignment textAlignment)
    {
        public System.Windows.TextAlignment ToWpfTextAlignment()
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
}
