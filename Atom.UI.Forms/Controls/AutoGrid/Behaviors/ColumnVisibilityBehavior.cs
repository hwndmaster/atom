using System.Windows.Controls;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnVisibilityBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (context.BuildColumn.Visibility is not null)
        {
            // Using the DataGrid.DataContext, previously attached via a resource
            var resource = context.DataGrid.FindResource("proxy");
            BindingOperations.SetBinding(context.Args.Column, DataGridColumn.VisibilityProperty,
                new Binding("Data." + context.BuildColumn.Visibility)
                {
                    Converter = new BooleanToVisibilityConverter(),
                    Source = resource
                });
        }
    }
}
