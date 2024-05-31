using System.Windows.Controls;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal static class SelectableBehavior
{
    public static void Attach(DataGrid dataGrid, Style rowStyle)
    {
        var listItemType = Helpers.GetListItemType(dataGrid.ItemsSource);
        if (dataGrid.SelectionMode == DataGridSelectionMode.Extended &&
            typeof(ISelectable).IsAssignableFrom(listItemType))
        {
            BindIsSelected(rowStyle);
        }
    }

    private static void BindIsSelected(Style style)
    {
        var binding = new Binding(nameof(ISelectable.IsSelected));
        style.Setters.Add(new Setter(DataGrid.IsSelectedProperty, binding));
    }
}
