namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal class ColumnDisplayIndexBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var index = context.GetAttribute<DisplayIndexAttribute>()?.Index;

        if (index.HasValue)
        {
            context.Args.Column.DisplayIndex = index.Value;
        }
    }
}
