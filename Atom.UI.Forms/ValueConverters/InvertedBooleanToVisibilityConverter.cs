using System.Windows;

namespace Genius.Atom.UI.Forms.ValueConverters
{
    public sealed class InvertedBooleanToVisibilityConverter : MarkupBooleanConverterBase<Visibility>
    {
        public InvertedBooleanToVisibilityConverter()
            : base(Visibility.Collapsed, Visibility.Visible) { }
    }
}
