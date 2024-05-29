using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Wpf.Builders;

namespace Genius.Atom.UI.Forms.Tests.Wpf.Builders;

public sealed class DataGridTextColumnBuilderTests
{
    [StaFact]
    public void HappyFlowScenario()
    {
        // Arrange
        const string propertyName = "Foo";
        var columnBuilder = DataGridColumnBuilder.ForValuePath(propertyName);
        var sut = new DataGridTextColumnBuilder(columnBuilder);

        // Act
        var column = sut.Build();

        // Verify
        Assert.NotNull(column);
        Assert.Equal(propertyName, column.Header);
        Assert.Equal(propertyName, column.SortMemberPath);
        Assert.NotNull(column.CellTemplate);
        Assert.Equal(typeof(TextBlock), column.CellTemplate.VisualTree.Type);

        // TODO: Assert bindings of TextBlock
    }

    [StaFact]
    public void WithTextHighlighting_HappyFlowScenario()
    {
        // Arrange
        const string propertyName = "Foo";
        var hlBinding1 = new Binding("Bar");
        var hlBinding2 = new Binding("Que");
        var columnBuilder = DataGridColumnBuilder.ForValuePath(propertyName);
        var sut = new DataGridTextColumnBuilder(columnBuilder)
            .WithTextHighlighting([hlBinding1, hlBinding2]);

        // Act
        var column = sut.Build();

        // Verify
        Assert.NotNull(column);
        Assert.Equal(propertyName, column.Header);
        Assert.Equal(propertyName, column.SortMemberPath);
        Assert.NotNull(column.CellTemplate);
        Assert.Equal(typeof(ContentControl), column.CellTemplate.VisualTree.Type);

        // TODO: Assert bindings of ContentControl
    }
}
