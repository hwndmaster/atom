using System.Windows.Data;

namespace Genius.Atom.UI.Forms;

internal sealed class NotNullToVisibilityConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
