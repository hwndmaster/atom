using System.ComponentModel.DataAnnotations;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors
{
    public class ColumnFormattingBehavior : IAutoGridColumnBehavior
    {
        public void Attach(AutoGridColumnContext context)
        {
            var format = context.GetAttribute<DisplayFormatAttribute>();
            if (format == null)
            {
                return;
            }

            context.GetBinding().StringFormat = format.DataFormatString;
        }
    }
}
