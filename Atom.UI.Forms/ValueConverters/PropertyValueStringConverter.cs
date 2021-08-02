using System;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.ValueConverters
{
    public class PropertyValueStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(string))
            {
                return value == null ? string.Empty : value.ToString();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            if (targetType.IsAssignableFrom(value.GetType()))
                return value;

            throw new NotSupportedException();
        }
    }
}
