using Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;
using Genius.Atom.UI.Forms.Tests.Controls.AutoGrid.Behaviors;

namespace Genius.Atom.UI.Forms.Tests.Validation;

public sealed class FilteringBehaviorTests : IDisposable
{
    private readonly FilteringBehavior _sut;
    private readonly BehaviorTestContext _behaviorTestContext = new();

    public FilteringBehaviorTests()
    {
        _sut = new FilteringBehavior(_behaviorTestContext.DataGrid, _behaviorTestContext.BuildContext, _behaviorTestContext.CollectionViewSource);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    [StaFact]
    public void HappyFlowScenario()
    {
        // Arrange
        _sut.Attach();
        _behaviorTestContext.Items.Add(_behaviorTestContext.Items[2]);

        // Act
        _behaviorTestContext.DataGridViewModel.Filter = _behaviorTestContext.Items[1].FilterableField1!;

        // Verify
        var result = _behaviorTestContext.CollectionViewSource.View.Cast<BehaviorTestContext.DummyItemViewModel>().ToArray();
        Assert.Single(result);
        Assert.Equal(_behaviorTestContext.Items[1], result[0]);

        // Act #2
        _behaviorTestContext.DataGridViewModel.Filter = _behaviorTestContext.Items[2].FilterableField2!;

        // Verify #2
        result = _behaviorTestContext.CollectionViewSource.View.Cast<BehaviorTestContext.DummyItemViewModel>().ToArray();
        Assert.Equal(2, result.Length);
        Assert.Equal(_behaviorTestContext.Items[2], result[0]);
        Assert.Equal(_behaviorTestContext.Items[3], result[1]);
    }
}
