using System.ComponentModel;
using System.Windows.Input;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal static class AutoGridBuilderHelpers
{
    public static IAutoGridContextBuilderColumn CreateContextBuilderColumn(PropertyDescriptor property)
    {
        Guard.NotNull(property);

        if (IsCommandColumn(property))
        {
            return new AutoGridContextBuilderCommandColumn(property);
        }

        return new AutoGridContextBuilderTextColumn(property);
    }

    public static bool IsCommandColumn(PropertyDescriptor property)
    {
        return typeof(ICommand).IsAssignableFrom(property.PropertyType);
    }
}
