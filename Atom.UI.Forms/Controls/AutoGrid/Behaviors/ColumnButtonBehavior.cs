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
        var iconSize = context.BuildCommandColumn?.IconSize;

        context.Args.Column = DataGridColumnBuilder.ForValuePath(context.Property.Name)
            .BasedOnAutoGridColumnContext(context)
            .RenderAsButton(icon, iconSize)
            .Build();
    }
}
