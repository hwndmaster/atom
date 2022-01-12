using System.Windows.Controls;
using System.Windows.Input;
using Genius.Atom.UI.Forms.Controls.TagEditor;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnTagEditorBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (!context.Property.PropertyType.IsAssignableFrom(typeof(TagEditorViewModel)))
        {
            return;
        }

        context.Args.Column = WpfHelpers.CreateTagEditorColumn(context.Property.Name,
            context.Property.Name);

        context.DataGrid.CellEditEnding += (object? sender, DataGridCellEditEndingEventArgs args) => {
            if (args.Column == context.Args.Column)
            {
                var elementContext = args.EditingElement.DataContext;

                if (elementContext is IHasDirtyFlag dirtyFlagContext)
                {
                    var tagsProp = elementContext.GetType()
                        .GetProperty(context.Property.Name)
                        .NotNull()
                        .GetValue(elementContext) as IHasDirtyFlag;
                    if (tagsProp?.IsDirty == true)
                    {
                        dirtyFlagContext.IsDirty = true;
                    }
                }
            }
        };

        context.DataGrid.PreparingCellForEdit += (object? sender, DataGridPreparingCellForEditEventArgs args) => {
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
            if (context.DataGrid.CurrentColumn == context.Args.Column
                && args.Key == Key.Enter)
            {
                var cellContent = context.Args.Column.GetCellContent(context.DataGrid.CurrentItem);
                var textBox = WpfHelpers.FindVisualChildren<AutoCompleteBox>(cellContent).First();
                if (textBox.Text != string.Empty)
                {
                    args.Handled = true;
                }
            }
        };
    }
}
