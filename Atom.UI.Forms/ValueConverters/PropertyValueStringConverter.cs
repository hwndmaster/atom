using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms;

public sealed class PropertyValueStringConverter : IValueConverter
{
    private readonly string? _displayFormat;
    private readonly string? _arraySeparator;

    public PropertyValueStringConverter(string? displayFormat, string? arraySeparator = null)
    {
        _displayFormat = displayFormat;
        _arraySeparator = arraySeparator ?? Environment.NewLine;
    }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType == typeof(string))
        {
            if (value is null)
            {
                return string.Empty;
            }

            if (value.GetType().IsArray)
            {
                var objArray = (IEnumerable)value;
                return string.Join(_arraySeparator, objArray.Cast<object>().Select(x => Convert(x, targetType, parameter, culture)?.ToString()));
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

        try
        {
            return System.Convert.ChangeType(value, targetType);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
