using Genius.Atom.UI.Forms.Wpf;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.ColumnBehaviors;

internal sealed class ColumnStylingBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var style = context.BuildColumn.Style;
        if (style is null)
        {
            return;
        }

        if (style.HorizontalAlignment is null)
        {
            style = style with { HorizontalAlignment = HorizontalAlignment.Left };
        }

        StylingHelpers.EnsureDefaultCellStyle(context.Args.Column);
        StylingHelpers.SetStyling(context.Args.Column.CellStyle, style);
    }
}
