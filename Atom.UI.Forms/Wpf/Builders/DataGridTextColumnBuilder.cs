using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Wpf.Builders;

internal sealed class DataGridTextColumnBuilder : DataGridColumnBuilder
{
    internal DataGridTextColumnBuilder(DataGridColumnBuilder parentBuilder)
        : base(parentBuilder.NotNull())
    {
    }

    public override DataGridTemplateColumn Build()
    {
        var column = CreateColumn();
        var binding = CreateBinding();

        var textFactory = new FrameworkElementFactory(typeof(TextBlock));
        textFactory.SetBinding(TextBlock.TextProperty, binding);

        column.CellTemplate = new DataTemplate { VisualTree = textFactory };

        return column;
    }
}
