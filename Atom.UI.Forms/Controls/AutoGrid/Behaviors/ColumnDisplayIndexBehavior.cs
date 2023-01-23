namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnDisplayIndexBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var index = context.BuildColumn.DisplayIndex;

        if (index.HasValue)
        {
            context.Args.Column.DisplayIndex = index.Value;
        }
    }
}
