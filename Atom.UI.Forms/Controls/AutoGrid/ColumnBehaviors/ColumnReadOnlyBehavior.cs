namespace Genius.Atom.UI.Forms.Controls.AutoGrid.ColumnBehaviors;

internal sealed class ColumnReadOnlyBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (context.BuildColumn.IsReadOnly)
        {
            context.Args.Column.IsReadOnly = true;
        }
    }
}
