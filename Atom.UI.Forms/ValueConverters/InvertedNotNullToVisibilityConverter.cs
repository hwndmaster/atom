using System.Windows.Data;
using System.Windows.Markup;

namespace Genius.Atom.UI.Forms;

internal sealed class InvertedNotNullToVisibilityConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
