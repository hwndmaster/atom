namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnStylingBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var style = context.GetAttribute<StyleAttribute>();
        if (style == null)
        {
            return;
        }

        WpfHelpers.SetCellHorizontalAlignment(context.Args.Column, style.HorizontalAlignment);
    }
}
