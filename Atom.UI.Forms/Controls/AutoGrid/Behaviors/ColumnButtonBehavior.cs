using System.Windows.Input;
using Genius.Atom.UI.Forms.Wpf.Builders;

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

        context.Args.Column = DataGridColumnBuilder.ForValuePath(context.Property.Name)
            .WithCellStyling(context.BuildColumn.Style)
            .RenderAsButton(icon)
            .Build();
    }
}
