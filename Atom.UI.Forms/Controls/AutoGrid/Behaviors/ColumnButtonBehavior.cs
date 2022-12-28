using System.Windows.Input;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnButtonBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (!typeof(ICommand).IsAssignableFrom(context.Property.PropertyType))
        {
            return;
        }

        var icon = context.BuildCommandColumn?.Icon;

        context.Args.Column = WpfHelpers.CreateButtonColumn(context.Property.Name, context.BuildColumn.Style, icon);
    }
}
