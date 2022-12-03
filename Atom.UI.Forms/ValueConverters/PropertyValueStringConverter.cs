using System.Globalization;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms;

public sealed class PropertyValueStringConverter : IValueConverter
{
    private readonly string? _displayFormat;

    public PropertyValueStringConverter(string? displayFormat)
    {
        _displayFormat = displayFormat;
    }

    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (targetType == typeof(string))
        {
            if (value is null)
            {
                return string.Empty;
            }

            if (value is IFormattable formattable && _displayFormat is not null)
            {
                return formattable.ToString(_displayFormat, CultureInfo.CurrentCulture);
            }

            return value.ToString();
        }

        return value;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
            return null;
        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        throw new NotSupportedException();
    }
}
