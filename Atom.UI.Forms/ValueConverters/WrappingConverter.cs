using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Genius.Atom.UI.Forms;

internal sealed class WrappingConverter : MarkupExtension, IMultiValueConverter
{
    private readonly PropertyValueStringConverter _defaultConverter = new(null);

    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2)
            return values.FirstOrDefault();

        var value = values[0];
        if (values[1] is not IValueConverter converter)
            return _defaultConverter.Convert(value, targetType, parameter, culture);

        return converter.Convert(value, targetType, parameter, culture);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
