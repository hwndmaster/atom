using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnAutoWidthBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (context.Args.Column.Visibility != Visibility.Visible
            || !context.Args.Column.Width.IsAuto)
        {
            return;
        }

        if (context.BuildColumn.AutoWidth)
        {
            context.Args.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }
    }
}
