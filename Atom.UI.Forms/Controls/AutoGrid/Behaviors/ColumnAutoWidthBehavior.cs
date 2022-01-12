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

        var greedyAttr = context.GetAttribute<GreedyAttribute>();
        if (greedyAttr is not null)
        {
            context.Args.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }
    }
}
