using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Genius.Atom.UI.Forms.Controls.TagEditor;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors
{
    internal class ColumnTagEditorBehavior : IAutoGridColumnBehavior
    {
        private const string FLAG = nameof(ColumnTagEditorBehavior);

        public void Attach(AutoGridColumnContext context)
        {
            if (!context.Property.PropertyType.IsAssignableFrom(typeof(TagEditorViewModel)))
            {
                return;
            }

            context.Args.Column = WpfHelpers.CreateTagEditorColumn(context.Property.Name,
                context.Property.Name);

            if (!context.Flags.Contains(FLAG))
            {
                context.Flags.Add(FLAG);
                context.DataGrid.PreparingCellForEdit += (object sender, DataGridPreparingCellForEditEventArgs args) => {
                    if (args.Column == context.Args.Column)
                    {
                        var textBox = WpfHelpers.FindVisualChildren<AutoCompleteBox>(args.EditingElement).FirstOrDefault();
                        if (textBox != null)
                        {
                            FocusManager.SetFocusedElement(args.EditingElement, textBox);
                        }
                    }
                };

                context.DataGrid.PreviewKeyDown += (object sender, KeyEventArgs args) => {
                    if (args.Key == Key.Enter)
                    {
                        args.Handled = true;
                    }
                };
            }
        }
    }
}
