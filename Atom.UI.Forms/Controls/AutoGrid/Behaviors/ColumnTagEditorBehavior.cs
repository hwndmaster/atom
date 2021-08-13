using System.Windows.Input;
using Genius.Atom.UI.Forms.Controls.TagEditor;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors
{
    public class ColumnTagEditorBehavior : IAutoGridColumnBehavior
    {
        public void Attach(AutoGridColumnContext context)
        {
            if (!typeof(TagEditorViewModel).IsAssignableFrom(context.Property.PropertyType))
            {
                return;
            }

            context.Args.Column = WpfHelpers.CreateTagEditorColumn(context.Property.Name,
                context.Property.Name);

            context.DataGrid.PreviewKeyDown += (object sender, KeyEventArgs args) => {
                if (args.Key == Key.Enter)
                {
                    args.Handled = true;
                }
            };
        }
    }
}
