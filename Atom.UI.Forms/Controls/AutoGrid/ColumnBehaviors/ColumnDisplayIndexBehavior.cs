namespace Genius.Atom.UI.Forms.Controls.AutoGrid.ColumnBehaviors;

internal sealed class ColumnDisplayIndexBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var index = context.BuildColumn.DisplayIndex;

        if (index.HasValue)
        {
            while (context.DataGrid.Columns.Any(x => x != context.Args.Column && x.DisplayIndex == index))
            {
                index++;
            }

            context.Args.Column.DisplayIndex = index.Value;
        }
    }
}
