using Genius.Atom.UI.Forms.Wpf.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnToggleButtonBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (!typeof(bool).IsAssignableFrom(context.Property.PropertyType)
            || context.BuildToggleButtonColumn is null)
        {
            return;
        }

        var iconForTrue = context.BuildToggleButtonColumn.IconForTrue;
        var iconForFalse = context.BuildToggleButtonColumn.IconForFalse;

        var columnBuilder = DataGridColumnBuilder.ForValuePath(context.Property.Name)
            .BasedOnAutoGridColumnContext(context);

        if (iconForTrue is not null && iconForFalse is not null)
        {
            context.Args.Column = columnBuilder
                .RenderAsToggleImageButton(iconForTrue, iconForFalse)
                .Build();
        }
        else
        {
            context.Args.Column = columnBuilder
                .RenderAsToggleSwitch()
                .Build();
        }
    }
}
