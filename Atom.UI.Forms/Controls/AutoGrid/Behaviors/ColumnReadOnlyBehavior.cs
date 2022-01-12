using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnReadOnlyBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var isReadOnly = context.GetAttribute<ReadOnlyAttribute>()?.IsReadOnly;

        if (isReadOnly == true)
        {
            context.Args.Column.IsReadOnly = true;
        }
    }
}
