using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Wpf.Builders;

internal sealed class DataGridViewContentColumnBuilder : DataGridColumnBuilder
{
    private readonly Type? _viewType;

    internal DataGridViewContentColumnBuilder(DataGridColumnBuilder parentBuilder, Type viewType)
        : base(parentBuilder.NotNull())
    {
        _viewType = viewType;
    }

    public override DataGridTemplateColumn Build()
    {
        var column = CreateColumn();
        var binding = CreateBinding();

        var viewContentFactory = new FrameworkElementFactory(_viewType);
        viewContentFactory.SetBinding(FrameworkElement.DataContextProperty, binding);
        column.CellTemplate = new DataTemplate { VisualTree = viewContentFactory };

        return column;
    }
}
