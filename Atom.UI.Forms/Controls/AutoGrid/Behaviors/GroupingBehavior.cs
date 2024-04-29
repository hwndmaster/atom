using System.Collections;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class GroupingBehavior : IDisposable
{
    private readonly Disposer _disposer = new();
    private readonly DataGrid _dataGrid;
    private readonly AutoGridBuildContext _buildContext;
    private readonly CollectionViewSource _collectionViewSource;
    private readonly IWpfApplication _wpfApplication;

    public GroupingBehavior(DataGrid dataGrid, AutoGridBuildContext buildContext, CollectionViewSource collectionViewSource)
    {
        _dataGrid = dataGrid.NotNull();
        _buildContext = buildContext.NotNull();
        _collectionViewSource = collectionViewSource.NotNull();

        _wpfApplication = Module.ServiceProvider.GetRequiredService<IWpfApplication>();
    }

    public GroupingBehavior Attach()
    {
        if (_buildContext.GroupByProperties.Length > 0)
        {
            InitializePredefinedColumnsGrouping();
        }
        else if (_buildContext.OptionalGroupingValueProperty is not null)
        {
            InitializeOptionalGrouping();
        }

        return this;
    }

    public void Dispose()
    {
        _disposer.Dispose();
    }

    private void InitializeOptionalGrouping()
    {
        var vm = _dataGrid.GetViewModel();
        vm.WhenChanged(_buildContext.OptionalGroupingSwitchProperty!, (bool doGrouping) => {
            _collectionViewSource.GroupDescriptions.Clear();
            if (!doGrouping)
                return;
            _collectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(_buildContext.OptionalGroupingValueProperty));
        }).DisposeWith(_disposer);

        EnableStructuredGroupStyle();
    }

    private void InitializePredefinedColumnsGrouping()
    {
        if (_collectionViewSource.Source is IEnumerable enumerable)
        {
            // Attach current items
            AttachToPropertyChangedEvents(enumerable);

            // Ensure all new items will be attached
            var observableCollection = _collectionViewSource.Source as INotifyCollectionChanged;
            if (observableCollection is not null)
            {
                observableCollection.WhenCollectionChanged()
                    .Subscribe(args =>
                    {
                        if (args.Action == NotifyCollectionChangedAction.Add)
                        {
                            AttachToPropertyChangedEvents(args.NewItems!);
                        }
                    }).DisposeWith(_disposer);
            }
        }

        foreach (var groupByProp in _buildContext.GroupByProperties)
        {
            _collectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(groupByProp.Property.Name));
        }

        if (_buildContext.GroupByProperties.Any(x => AutoGridBuilderHelpers.IsGroupableColumn(x.Property)))
        {
            EnableStructuredGroupStyle();
        }
        else
        {
            EnableSimpleGroupStyle();
        }
    }

    private void AttachToPropertyChangedEvents(IEnumerable items)
    {
        foreach (var childViewModel in items.OfType<ViewModelBase>())
        {
            foreach (var groupByProp in _buildContext.GroupByProperties)
            {
                childViewModel.WhenChanged(groupByProp.Property.Name, (object _) =>
                    _collectionViewSource.View.Refresh())
                    .DisposeWith(_disposer);
            }
        }
    }

    private void EnableStructuredGroupStyle()
    {
        _dataGrid.GroupStyle.Add(_wpfApplication.FindResource<GroupStyle>("Atom.AutoGrid.Group.GroupableViewModel"));
        _dataGrid.SetValue(Grid.IsSharedSizeScopeProperty, true);
    }

    private void EnableSimpleGroupStyle()
    {
        _dataGrid.GroupStyle.Add(_wpfApplication.FindResource<GroupStyle>("Atom.AutoGrid.Group.String"));
    }
}
