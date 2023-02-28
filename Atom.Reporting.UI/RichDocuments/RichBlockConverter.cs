using System.Windows.Documents;
using System.Windows.Media;
using Genius.Atom.Reporting.RichDocuments;

namespace Genius.Atom.Reporting.UI.RichDocuments;

public abstract class RichBlockConverter : RichBlockBaseConverter, IRichBlockConverter
{
    public abstract bool CanConvert(RichBlock block);
    public abstract Block Convert(RichBlock block);

    protected void ConvertBlockProperties(RichBlock source, Block target)
    {
        base.ConvertBlockBaseProperties(source, target);

        if (source.BorderColor is not null)
        {
            target.BorderBrush = new SolidColorBrush(source.BorderColor.Value.ToWpfColor());
        }
        if (source.BorderThickness is not null)
        {
            target.BorderThickness = source.BorderThickness.Value.ToWpfThickness();
        }
        if (source.Padding is not null)
        {
            target.Padding = source.Padding.Value.ToWpfThickness();
        }
        if (source.Margin is not null)
        {
            target.Margin = source.Margin.Value.ToWpfThickness();
        }
        if (source.Margin is not null)
        {
            target.Margin = source.Margin.Value.ToWpfThickness();
        }
        if (source.LineHeight is not null)
        {
            target.LineHeight = source.LineHeight.Value;
        }
        if (source.TextAlignment is not null)
        {
            target.TextAlignment = source.TextAlignment.Value.ToWpfTextAlignment();
        }
    }
}
