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

        context.Args.Column = WpfHelpers.CreateToggleSwitchColumn(context.Property.Name, iconForTrue, iconForFalse, context.BuildColumn.Style);
    }
}
