using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.Infrastructure;
using Genius.Atom.Infrastructure.TestingUtil;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Genius.Atom.UI.Forms.TestingUtil;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Tests.Controls.AutoGrid.Behaviors;

internal sealed class BehaviorTestContext
{
    static BehaviorTestContext()
    {
        TestModule.Initialize();

        TestModule.ServiceProvider.GetRequiredService<TestWpfApplication>()
            .AddSampleResources("Atom.AutoGrid.Group.String", new GroupStyle())
            .AddSampleResources("Atom.AutoGrid.Group.GroupableViewModel", new GroupStyle());
    }

    public BehaviorTestContext()
    {
        DataGridViewModel = new DummyViewModel();
        DataGrid = new DataGrid
        {
            DataContext = DataGridViewModel
        };
        Items = new ObservableCollection<DummyItemViewModel>(Fixture.CreateMany<DummyItemViewModel>().ToList());
        CollectionViewSource = new CollectionViewSource
        {
            Source = Items
        };

        InitializeBuildCOntext();
    }

    [MemberNotNull(nameof(BuildContext))]
    public void InitializeBuildCOntext()
    {
        var properties = TypeDescriptor.GetProperties(typeof(DummyItemViewModel));
        var filterableField1 = properties.Find(nameof(DummyItemViewModel.FilterableField1), false).NotNull();
        var filterableField2 = properties.Find(nameof(DummyItemViewModel.FilterableField2), false).NotNull();
        var groupableField1 = properties.Find(nameof(DummyItemViewModel.GroupableField1), false).NotNull();
        var groupableField2 = properties.Find(nameof(DummyItemViewModel.GroupableField2), false).NotNull();
        var columns = new List<AutoGridBuildColumnContext>()
        {
            new AutoGridBuildTextColumnContext(filterableField1, AutoGridContextBuilderBaseFields.Default)
            {
                Filterable = true
            },
            new AutoGridBuildTextColumnContext(filterableField2, AutoGridContextBuilderBaseFields.Default)
            {
                Filterable = true
            },
            new AutoGridBuildTextColumnContext(groupableField1, AutoGridContextBuilderBaseFields.Default)
            {
                IsGrouped = true
            },
            new AutoGridBuildTextColumnContext(groupableField2, AutoGridContextBuilderBaseFields.Default)
            {
                IsGrouped = true
            }
        };

        BuildContext = new AutoGridBuildContext(columns, new DefaultFactory<object>(() => throw new Exception()));
    }

    public void InitializeBuildContextForOptionalGrouping()
    {
        var properties = TypeDescriptor.GetProperties(typeof(DummyItemViewModel));
        var columns = new List<AutoGridBuildColumnContext>()
        {
            // Add random columns, doesn't matter which ones:
            new AutoGridBuildTextColumnContext(properties[0], AutoGridContextBuilderBaseFields.Default),
            new AutoGridBuildTextColumnContext(properties[1], AutoGridContextBuilderBaseFields.Default)
        };

        BuildContext = new AutoGridBuildContext(columns, new DefaultFactory<object>(() => throw new Exception()))
        {
            OptionalGroupingSwitchProperty = nameof(DummyViewModel.DoGrouping),
            OptionalGroupingValueProperty = nameof(DummyItemViewModel.GroupableField3)
        };
    }

    public IFixture Fixture { get; } = InfrastructureTestHelper.CreateFixture();
    public DataGrid DataGrid { get; }
    public DummyViewModel DataGridViewModel { get; }
    public AutoGridBuildContext BuildContext { get; private set; }
    public CollectionViewSource CollectionViewSource { get; }
    public ObservableCollection<DummyItemViewModel> Items { get;}

    internal class DummyViewModel : ViewModelBase
    {
        [FilterContext]
        public string Filter
        {
            get => GetOrDefault<string>();
            set => RaiseAndSetIfChanged(value);
        }

        public bool DoGrouping
        {
            get => GetOrDefault(false);
            set => RaiseAndSetIfChanged(value);
        }
    }

    internal class DummyItemViewModel : ViewModelBase
    {
        public string? FilterableField1 { get; set; }
        public string? FilterableField2 { get; set; }

        public string? GroupableField1
        {
            get => GetOrDefault<string>();
            set => RaiseAndSetIfChanged(value);
        }

        public string? GroupableField2
        {
            get => GetOrDefault<string>();
            set => RaiseAndSetIfChanged(value);
        }

        public IGroupableViewModel? GroupableField3 { get; set; }
    }
}
