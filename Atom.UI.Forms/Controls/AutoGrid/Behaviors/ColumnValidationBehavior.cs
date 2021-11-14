using System.Windows;
using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal class ColumnValidationBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (context.DataGrid.IsReadOnly || context.Args.Column.IsReadOnly)
        {
            return;
        }

        var columnBinding = context.GetBinding();
        if (columnBinding is null)
        {
            return;
        }

        //columnBinding.ValidatesOnDataErrors = true;
        columnBinding.ValidatesOnNotifyDataErrors = true;
        columnBinding.NotifyOnValidationError = true;
        if (context.Args.Column is DataGridTextColumn textColumn)
        {
            textColumn.ElementStyle = (Style)Application.Current.FindResource("ValidatableTextCellElementStyle");
        }
        else if (context.Args.Column is DataGridCheckBoxColumn checkBoxColumn)
        {
            checkBoxColumn.ElementStyle = (Style)Application.Current.FindResource("ValidatableCheckboxCellElementStyle");
        }
        else
        {
            throw new NotSupportedException($"This column type is not supported: {context.Args.Column.GetType().Name}");
        }
    }
}
