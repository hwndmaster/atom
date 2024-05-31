using System.Windows.Controls;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal static class EditableBehavior
{
    public static void Attach(DataGrid dataGrid, Style rowStyle)
    {
        var listItemType = Helpers.GetListItemType(dataGrid.ItemsSource);
        if (!typeof(IEditable).IsAssignableFrom(listItemType))
            return;

        var binding = new Binding(nameof(IEditable.IsEditing));
        rowStyle.Setters.Add(new Setter(Properties.IsEditingProperty, binding));

        dataGrid.BeginningEdit += (sender, e) =>
        {
            if (e.Row.Item is IEditable editable)
            {
                e.Row.SetValue(Properties.IsEditingHandlingSuspendedProperty, true);
                editable.IsEditing = true;
                e.Row.SetValue(Properties.IsEditingHandlingSuspendedProperty, false);
            }
        };
        dataGrid.RowEditEnding += (sender, e) =>
        {
            if (e.Row.Item is IEditable editable)
            {
                e.Row.SetValue(Properties.IsEditingHandlingSuspendedProperty, true);
                editable.IsEditing = false;
                e.Row.SetValue(Properties.IsEditingHandlingSuspendedProperty, false);
            }
        };
    }
}
