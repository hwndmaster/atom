using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.Infrastructure;
using Genius.Atom.Infrastructure.TestingUtil;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

namespace Genius.Atom.UI.Forms.Tests.Validation;

public sealed class FilteringBehaviorTests : IDisposable
{
    private readonly IFixture _fixture = InfrastructureTestHelper.CreateFixture();
    private readonly FilteringBehavior _sut;
    private readonly DummyViewModel _viewModel;
    private readonly List<DummyItemViewModel> _sampleItems;
    private readonly CollectionViewSource _collectionViewSource;

    public FilteringBehaviorTests()
    {
        _viewModel = new DummyViewModel();
        var dataGrid = new DataGrid
        {
            DataContext = _viewModel
        };
        var properties = TypeDescriptor.GetProperties(typeof(DummyItemViewModel));
        var columns = new List<AutoGridBuildColumnContext>()
        {
            new AutoGridBuildTextColumnContext(properties[0], AutoGridContextBuilderBaseFields.Default)
            {
                Filterable = true
            }
        };
        var buildContext = new AutoGridBuildContext(columns, new DefaultFactory<object>(() => throw new Exception()));
        _sampleItems = _fixture.CreateMany<DummyItemViewModel>().ToList();
        _collectionViewSource = new CollectionViewSource
        {
            Source = _sampleItems
        };
        _sut = new FilteringBehavior(dataGrid, buildContext, _collectionViewSource);
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

        // Act
        _viewModel.Filter = _sampleItems[1].SampleString!;

        // Verify
        var result = _collectionViewSource.View.Cast<DummyItemViewModel>().ToArray();
        Assert.Single(result);
        Assert.Equal(_sampleItems[1], result[0]);
    }

    class DummyViewModel : ViewModelBase
    {
        [FilterContext]
        public string Filter
        {
            get => GetOrDefault<string>();
            set => RaiseAndSetIfChanged(value);
        }
    }

    class DummyItemViewModel
    {
        public string? SampleString { get; set; }
        public int SampleInteger { get; set; }
    }
}
