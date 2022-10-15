namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnFormattingBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var format = context.BuildTextColumn?.DisplayFormat;
        if (format == null)
        {
            return;
        }

        context.GetBinding().NotNull().StringFormat = format;
    }
}
