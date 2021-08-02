using System.Windows.Input;
using Genius.Atom.UI.Forms.Attributes;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors
{
    public class ColumnDisplayIndexBehavior : IAutoGridColumnBehavior
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
}
