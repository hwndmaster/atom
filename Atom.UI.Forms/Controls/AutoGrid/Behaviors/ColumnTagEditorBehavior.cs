using System.Windows.Controls;
using System.Windows.Input;
using Genius.Atom.UI.Forms.Controls.TagEditor;
using Genius.Atom.UI.Forms.Wpf;
using Genius.Atom.UI.Forms.Wpf.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnTagEditorBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (!context.Property.PropertyType.IsAssignableFrom(typeof(TagEditorViewModel)))
        {
            return;
        }

        context.Args.Column = DataGridColumnBuilder.ForValuePath(context.Property.Name)
            .RenderAsTagEditor()
            .Build();

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
                var textBox = args.EditingElement.FindVisualChildren<AutoCompleteBox>().FirstOrDefault();
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
                var textBox = cellContent.FindVisualChildren<AutoCompleteBox>().First();
                if (textBox.Text != string.Empty)
                {
                    args.Handled = true;
                }
            }
        };
    }
}
