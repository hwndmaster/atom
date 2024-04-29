using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;
using Genius.Atom.UI.Forms.Tests.Controls.AutoGrid.Behaviors;

namespace Genius.Atom.UI.Forms.Tests.Validation;

public sealed class GroupingBehaviorTests
{
    [StaFact]
    public void PredefinedColumnsGrouping_HappyFlowScenario()
    {
        // Act
        var behaviorTestContext = new BehaviorTestContext();
        using var sut = new GroupingBehavior(behaviorTestContext.DataGrid, behaviorTestContext.BuildContext, behaviorTestContext.CollectionViewSource);
        sut.Attach();

        // Verify
        Assert.Equal(2, behaviorTestContext.CollectionViewSource.GroupDescriptions.Count);
        Assert.Equal(nameof(BehaviorTestContext.DummyItemViewModel.GroupableField1),
            ((PropertyGroupDescription)behaviorTestContext.CollectionViewSource.GroupDescriptions[0]).PropertyName);
        Assert.Equal(nameof(BehaviorTestContext.DummyItemViewModel.GroupableField2),
            ((PropertyGroupDescription)behaviorTestContext.CollectionViewSource.GroupDescriptions[1]).PropertyName);
    }

    [StaFact]
    public void GivenPredefinedColumnsGrouping_WhenGroupableItemPropertyChanged_ThenGroupingRefreshed()
    {
        // Arrange
        var behaviorTestContext = new BehaviorTestContext();
        using var sut = new GroupingBehavior(behaviorTestContext.DataGrid, behaviorTestContext.BuildContext, behaviorTestContext.CollectionViewSource);
        sut.Attach();

        // Pre-verify
        var actualGroups = behaviorTestContext.CollectionViewSource.View.Groups;
        Assert.Equal(3, actualGroups.Count);

        // Act
        behaviorTestContext.Items[1].GroupableField1 = behaviorTestContext.Items[0].GroupableField1;

        // Verify
        Assert.Equal(2, actualGroups.Count);
    }

    [StaFact]
    public void GivenPredefinedColumnsGrouping_WhenItemAdded_ThenSubscriptionAppliedToItemPropertyChanges()
    {
        // Arrange
        var behaviorTestContext = new BehaviorTestContext();
        using var sut = new GroupingBehavior(behaviorTestContext.DataGrid, behaviorTestContext.BuildContext, behaviorTestContext.CollectionViewSource);
        sut.Attach();
        var itemToAdd = behaviorTestContext.Fixture.Create<BehaviorTestContext.DummyItemViewModel>();

        // Act
        behaviorTestContext.Items.Add(itemToAdd);

        // Verify
        var actualGroups = behaviorTestContext.CollectionViewSource.View.Groups;
        Assert.Equal(4, actualGroups.Count);

        // Act
        itemToAdd.GroupableField1 = behaviorTestContext.Items[0].GroupableField1;

        // Verify
        Assert.Equal(3, actualGroups.Count);
    }

    [StaFact]
    public void OptionalGroupingProvided_HappyFlowScenario()
    {
        // Arrange
        var behaviorTestContext = new BehaviorTestContext();
        behaviorTestContext.InitializeBuildContextForOptionalGrouping();
        behaviorTestContext.Items[0].GroupableField3 = new DefaultGroupableViewModel("Group 1");
        behaviorTestContext.Items[1].GroupableField3 = behaviorTestContext.Items[0].GroupableField3;
        behaviorTestContext.Items[2].GroupableField3 = new DefaultGroupableViewModel("Group 2");
        using var sut = new GroupingBehavior(behaviorTestContext.DataGrid, behaviorTestContext.BuildContext, behaviorTestContext.CollectionViewSource);
        sut.Attach();

        // Pre-Verify
        Assert.Empty(behaviorTestContext.CollectionViewSource.GroupDescriptions);
        Assert.Null(behaviorTestContext.CollectionViewSource.View.Groups);

        // Act
        behaviorTestContext.DataGridViewModel.DoGrouping = true;

        // Verify
        Assert.Single(behaviorTestContext.CollectionViewSource.GroupDescriptions);
        Assert.Equal(2, behaviorTestContext.CollectionViewSource.View.Groups!.Count);

        // Act #2
        behaviorTestContext.DataGridViewModel.DoGrouping = false;

        // Verify #2
        Assert.Empty(behaviorTestContext.CollectionViewSource.GroupDescriptions);
        Assert.Null(behaviorTestContext.CollectionViewSource.View.Groups);
    }
}
