using System.ComponentModel.DataAnnotations;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal class ColumnFormattingBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var format = context.GetAttribute<DisplayFormatAttribute>();
        if (format == null)
        {
            return;
        }

        context.GetBinding().NotNull().StringFormat = format.DataFormatString;
    }
}
