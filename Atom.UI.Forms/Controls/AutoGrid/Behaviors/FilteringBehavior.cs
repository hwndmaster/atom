using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

// TODO: Cover with unit tests
internal sealed class FilteringBehavior
{
    public void Attach(DataGrid dataGrid, AutoGridBuildContext buildContext,
        CollectionViewSource collectionViewSource)
    {
        var vm = GetViewModel(dataGrid);

        var filterContext = buildContext.FilterContextScope is null
            ? Array.Find(vm.GetType().GetProperties(), x => x.GetCustomAttributes(false).OfType<FilterContextAttribute>().Any())
            : Array.Find(vm.GetType().GetProperties(), x => x.GetCustomAttributes(false).OfType<FilterContextAttribute>()
                .Any(x => buildContext.FilterContextScope.Equals(x.Scope, StringComparison.Ordinal)));

        if (filterContext is null || buildContext.FilterByProperties.Length == 0)
            return;

        string filter = string.Empty;
        // TODO: Dispose event subscription when detached
        vm.WhenChanged(filterContext.Name, (string s) => {
            filter = s;
            collectionViewSource.View.Refresh();
        });

        // TODO: Dispose event subscription when detached
        collectionViewSource.Filter += (object sender, FilterEventArgs e) =>
        {
            if (string.IsNullOrEmpty(filter))
            {
                return;
            }

            foreach (var filterProp in buildContext.FilterByProperties)
            {
                var value = filterProp.Property.GetValue(e.Item);

                if (AutoGridRowFilter.IsMatch(value, filter, filterProp.ValueConverter))
                {
                    return;
                }
            }

            e.Accepted = false;
        };
    }

    private static ViewModelBase GetViewModel(DataGrid dataGrid)
    {
        return dataGrid.DataContext as ViewModelBase
            ?? throw new InvalidCastException($"Cannot cast DataContext to {nameof(ViewModelBase)}");
    }
}
