using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Genius.Atom.UI.Forms.TestingUtil;
using Genius.Atom.UI.Forms.Wpf.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Tests.Wpf.Builders;

public sealed class HighlightedTextConverterTests
{
    private static Style _highlightStyle = new();
    private readonly DataGridTextColumnBuilder.HighlightedTextConverter _sut = new();

    static HighlightedTextConverterTests()
    {
        TestModule.Initialize();

        TestModule.ServiceProvider.GetRequiredService<TestWpfApplication>()
            .AddSampleResources("Atom.Run.Highlight", _highlightStyle);
    }

    [StaFact]
    public void GivenUseRegex_HappyFlowScenario()
    {
        // Arrange
        var value = new object[] { @"\d{2}\s\d{3}", true, "Test 12 345 test"};

        // Act
        var result = _sut.Convert(value, default!, default!, default!) as TextBlock;

        // Verify
        Assert.NotNull(result);
        Assert.Equal(3, result.Inlines.Count);
        var runs = result.Inlines.Cast<Run>().ToArray();
        Assert.Equal("Test ", runs[0].Text);
        Assert.Equal("12 345", runs[1].Text);
        Assert.Equal(" test", runs[2].Text);
        Assert.Equal(_highlightStyle, runs[1].Style);
    }

    [StaFact]
    public void GivenUseRegex_WhenNoMatches()
    {
        // Arrange
        var value = new object[] { @"\d{2}", true, "Lorem Ipsum"};

        // Act
        var result = _sut.Convert(value, default!, default!, default!) as TextBlock;

        // Verify
        Assert.NotNull(result);
        Assert.Single(result.Inlines);
        Assert.Equal("Lorem Ipsum", ((Run)result.Inlines.First()).Text);
        Assert.Equal("Lorem Ipsum", result.Text);
    }

    [StaFact]
    public void GivenNotRegex_HappyFlowScenario()
    {
        // Arrange
        var value = new object[] { "Test", false, "Lorem Test Ipsum Test Dolore"};

        // Act
        var result = _sut.Convert(value, default!, default!, default!) as TextBlock;

        // Verify
        Assert.NotNull(result);
        Assert.Equal(5, result.Inlines.Count);
        var runs = result.Inlines.Cast<Run>().ToArray();
        Assert.Equal("Lorem ", runs[0].Text);
        Assert.Equal("Test", runs[1].Text);
        Assert.Equal(" Ipsum ", runs[2].Text);
        Assert.Equal("Test", runs[3].Text);
        Assert.Equal(" Dolore", runs[4].Text);
        Assert.Equal(_highlightStyle, runs[1].Style);
        Assert.Equal(_highlightStyle, runs[3].Style);
    }
}
