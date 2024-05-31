using System.Diagnostics.CodeAnalysis;
using System.Windows.Documents;
using System.Windows.Media;
using Genius.Atom.Reporting.RichDocuments;

namespace Genius.Atom.Reporting.UI.RichDocuments;

public abstract class RichBlockBaseConverter
{
    protected static void ConvertBlockBaseProperties([NotNull] RichBlockBase source, [NotNull] TextElement target)
    {
        Guard.NotNull(source);
        Guard.NotNull(target);

        if (source.BackgroundColor is not null)
        {
            target.Background = new SolidColorBrush(source.BackgroundColor.Value.ToWpfColor());
        }
        if (source.FontFamily is not null)
        {
            target.FontFamily = new FontFamily(source.FontFamily);
        }
        if (source.FontSize is not null)
        {
            target.FontSize = source.FontSize.Value;
        }
        if (source.FontStretch is not null)
        {
            target.FontStretch = source.FontStretch switch
            {
                FontStretch.UltraCondensed => System.Windows.FontStretches.UltraCondensed,
                FontStretch.ExtraCondensed => System.Windows.FontStretches.ExtraCondensed,
                FontStretch.Condensed => System.Windows.FontStretches.Condensed,
                FontStretch.SemiCondensed => System.Windows.FontStretches.SemiCondensed,
                FontStretch.Medium => System.Windows.FontStretches.Medium,
                FontStretch.SemiExpanded => System.Windows.FontStretches.SemiExpanded,
                FontStretch.Expanded => System.Windows.FontStretches.Expanded,
                FontStretch.ExtraExpanded => System.Windows.FontStretches.ExtraExpanded,
                FontStretch.UltraExpanded => System.Windows.FontStretches.UltraExpanded,
                _ => throw new NotSupportedException("FontStretch is not supported: " + source.FontStretch)
            };
        }
        if (source.ForegroundColor is not null)
        {
            target.Foreground = new SolidColorBrush(source.ForegroundColor.Value.ToWpfColor());
        }
    }
}
