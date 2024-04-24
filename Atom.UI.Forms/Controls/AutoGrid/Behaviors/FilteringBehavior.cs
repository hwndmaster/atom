using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

// TODO: Cover with unit tests
internal sealed class FilteringBehavior : IDisposable
{
    private readonly Disposer _disposer = new();
    private readonly DataGrid _dataGrid;
    private readonly AutoGridBuildContext _buildContext;
    private readonly CollectionViewSource _collectionViewSource;
    private bool _isAttached = false;
    private string _filter = string.Empty;

    public FilteringBehavior(DataGrid dataGrid, AutoGridBuildContext buildContext, CollectionViewSource collectionViewSource)
    {
        _dataGrid = dataGrid.NotNull();
        _buildContext = buildContext.NotNull();
        _collectionViewSource = collectionViewSource.NotNull();
    }

    public FilteringBehavior Attach()
    {
        if (_isAttached)
            return this;
        _isAttached = true;

        if (_buildContext.FilterByProperties.Length == 0)
            return this;

        var vm = _dataGrid.GetViewModel();

        var filterContext = _buildContext.FilterContextScope is null
            ? Array.Find(vm.GetType().GetProperties(), x => x.GetCustomAttributes(false).OfType<FilterContextAttribute>().Any())
            : Array.Find(vm.GetType().GetProperties(), x => x.GetCustomAttributes(false).OfType<FilterContextAttribute>()
                .Any(x => _buildContext.FilterContextScope.Equals(x.Scope, StringComparison.Ordinal)));

        if (filterContext is null)
            return this;

        _disposer.Add(vm.WhenChanged(filterContext.Name, (string s) => {
            _filter = s;
            _collectionViewSource.View.Refresh();
        }));

        _collectionViewSource.Filter += OnCollectionViewSourceFilter;
        _disposer.Add(() => _collectionViewSource.Filter -= OnCollectionViewSourceFilter);

        return this;
    }

    public void Dispose()
    {
        _disposer.Dispose();
    }

    private void OnCollectionViewSourceFilter(object sender, FilterEventArgs e)
    {
        if (string.IsNullOrEmpty(_filter))
        {
            return;
        }

        foreach (var filterProp in _buildContext.FilterByProperties)
        {
            var value = filterProp.Property.GetValue(e.Item);

            if (AutoGridRowFilter.IsMatch(value, _filter, filterProp.ValueConverter))
            {
                return;
            }
        }

        e.Accepted = false;
    }
}
