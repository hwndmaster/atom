using System.Globalization;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

internal static class AutoGridRowFilter
{
    public static bool IsMatch(object? value, string filter, IValueConverter? converter)
    {
        if (value is IGroupableViewModel groupableVm)
        {
            value = groupableVm.GroupTitle;
        }

        if (converter is not null)
        {
            try
            {
                value = converter.Convert(value, typeof(string), null, CultureInfo.CurrentUICulture);
            }
            catch (Exception)
            {
                // Do nothing.
            }
        }

        if (value is not string && value is IFormattable formattable)
        {
            value = formattable.ToString(null, CultureInfo.InvariantCulture);
        }

        if (value is string stringValue)
        {
            return stringValue.Contains(filter, StringComparison.InvariantCultureIgnoreCase);
        }
        else
        {
            throw new NotSupportedException($"Type '{value?.GetType().Name}' is not supported yet for AutoGrid filtering");
        }
    }
}
