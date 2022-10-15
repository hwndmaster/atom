namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnHeaderNameBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (context.Args.Column.Header is not string)
        {
            return;
        }

        context.Args.Column.Header = context.BuildColumn.DisplayName;
    }
}
