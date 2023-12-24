using System.Windows.Controls;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

// TODO: Cover with unit tests
internal sealed class SelectableBehavior
{
    public void Attach(DataGrid dataGrid, Style rowStyle)
    {
        var listItemType = Helpers.GetListItemType(dataGrid.ItemsSource);
        if (dataGrid.SelectionMode == DataGridSelectionMode.Extended &&
            typeof(ISelectable).IsAssignableFrom(listItemType))
        {
            BindIsSelected(rowStyle);
        }
    }

    private void BindIsSelected(Style style)
    {
        var binding = new Binding(nameof(ISelectable.IsSelected));
        style.Setters.Add(new Setter(DataGrid.IsSelectedProperty, binding));
    }
}
