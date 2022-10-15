namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnStylingBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var style = context.BuildColumn.Style;
        if (style is null)
        {
            return;
        }

        WpfHelpers.SetCellHorizontalAlignment(context.Args.Column, style.HorizontalAlignment);
    }
}
