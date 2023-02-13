using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Genius.Atom.UI.Forms.Wpf;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

public static class Properties
{
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached(
        "ItemsSource",
        typeof(object),
        typeof(Properties),
        new PropertyMetadata(ItemsSourceChanged));

    public static readonly DependencyProperty AutoGridBuilderProperty = DependencyProperty.RegisterAttached(
        "AutoGridBuilder",
        typeof(IAutoGridBuilder),
        typeof(Properties),
        new PropertyMetadata());

    public static readonly DependencyProperty IsEditingProperty = DependencyProperty.RegisterAttached(
        "IsEditing",
        typeof(bool),
        typeof(Properties),
        new PropertyMetadata(IsEditingChanged));

    public static readonly DependencyProperty IsEditingHandlingSuspendedProperty = DependencyProperty.RegisterAttached(
        "IsEditingHandlingSuspended",
        typeof(bool),
        typeof(Properties));

    public static void SetItemsSource(DependencyObject element, object value)
    {
        element.SetValue(ItemsSourceProperty, value);
    }

    public static object GetItemsSource(DependencyObject element)
    {
        return element.GetValue(ItemsSourceProperty);
    }

    public static void SetAutoGridBuilder(DependencyObject element, IAutoGridBuilder? value)
    {
        element.SetValue(AutoGridBuilderProperty, value);
    }

    public static IAutoGridBuilder? GetAutoGridBuilder(DependencyObject element)
    {
        return (IAutoGridBuilder?) element.GetValue(AutoGridBuilderProperty);
    }

    private static void IsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGridRow row)
            return;

        if (d.GetValue(IsEditingHandlingSuspendedProperty) is true)
            return;

        DataGridCellsPresenter? presenter = row.FindVisualChildren<DataGridCellsPresenter>()
            .FirstOrDefault();
        if (presenter is null)
            return;

        const int column = 0;
        if (presenter.ItemContainerGenerator.ContainerFromIndex(column) is not DataGridCell cell)
            return;

        var grid = row.FindVisualParent<DataGrid>();
        if (grid is null)
            return;

        if ((bool)e.NewValue)
        {
            grid.ScrollIntoView(row, cell.Column);
            cell.Focus();

            grid.CurrentCell = new DataGridCellInfo(cell);

            Application.Current.Dispatcher.BeginInvoke(() => grid.BeginEdit(),
                DispatcherPriority.ApplicationIdle, null);
        }
        else
        {
            grid.EndInit();
        }
    }

    private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dataGrid = (DataGrid)d;
        var itemType = Helpers.GetListItemType(e.NewValue);
        var properties = itemType.GetProperties();
        var buildContext = AutoGridBuildContext.CreateLazy(dataGrid);

        var groupByProps = buildContext.Value.Columns.OfType<AutoGridBuildTextColumnContext>()
            .Where(x => x.IsGrouped)
            .ToArray();
        var filterByProps = buildContext.Value.Columns
            .OfType<AutoGridBuildTextColumnContext>()
            .Where(x => x.Filterable)
            .ToArray();

        if (groupByProps.Length == 0 && filterByProps.Length == 0)
        {
            d.SetValue(DataGrid.ItemsSourceProperty, e.NewValue is CollectionViewSource cvs
                ? cvs.View
                : e.NewValue);
        }
        else
        {
            var collectionViewSource = e.NewValue is CollectionViewSource cvs
                ? cvs
                : new CollectionViewSource
                {
                    Source = e.NewValue
                };

            SetupGrouping(groupByProps, collectionViewSource);
            SetupFiltering(d, filterByProps, collectionViewSource);

            d.SetValue(DataGrid.ItemsSourceProperty, collectionViewSource.View);
        }
    }

    private static void SetupGrouping(AutoGridBuildTextColumnContext[] groupByProps, CollectionViewSource collectionViewSource)
    {
        if (collectionViewSource.Source is IEnumerable enumerable)
        {
            // Attach current items
            AttachToPropertyChangedEvents(groupByProps, collectionViewSource, enumerable);

            // Ensure all new items will be attached
            var observableCollection = collectionViewSource.Source as ITypedObservableCollection;
            if (observableCollection is not null)
            {
                // TODO: Dispose event subscription when detached
                // TODO: Use ObservableExtensions.WhenCollectionChanged
                observableCollection.CollectionChanged += (sender, args) => {
                    if (args.Action == NotifyCollectionChangedAction.Add)
                    {
                        AttachToPropertyChangedEvents(groupByProps, collectionViewSource, args.NewItems!);
                    }
                };
            }
        }

        foreach (var groupByProp in groupByProps)
        {
            collectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(groupByProp.Property.Name));
        }
    }

    private static void AttachToPropertyChangedEvents(AutoGridBuildTextColumnContext[] groupByProps, CollectionViewSource collectionViewSource, IEnumerable items)
    {
        foreach (var childViewModel in items.OfType<ViewModelBase>())
        {
            foreach (var groupByProp in groupByProps)
            {
                childViewModel.WhenChanged(groupByProp.Property.Name, (object _) =>
                    collectionViewSource.View.Refresh());
            }
        }
    }

    private static void SetupFiltering(DependencyObject d, AutoGridBuildTextColumnContext[] filterByProps,
        CollectionViewSource collectionViewSource)
    {
        var vm = GetViewModel(d);

        var filterContext = vm.GetType().GetProperties()
            .FirstOrDefault(x => x.GetCustomAttributes(false).OfType<FilterContextAttribute>().Any());

        if (filterContext is null || filterByProps.Length == 0)
            return;

        string filter = string.Empty;
        // TODO: Dispose event subscription when detached
        // TODO: Use vm.WhenChanged() which returns IObservable<T>
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

            foreach (var filterProp in filterByProps)
            {
                var value = filterProp.Property.GetValue(e.Item);
                if (value is string stringValue)
                {
                    if (stringValue.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Accepted
                        return;
                    }
                }
                else
                {
                    throw new NotSupportedException($"Type '{value?.GetType().Name}' is not supported yet for AutoGrid filtering");
                }
            }

            e.Accepted = false;
        };
    }

    private static ViewModelBase GetViewModel(DependencyObject d)
    {
        return ((FrameworkElement)d).DataContext as ViewModelBase
            ?? throw new InvalidCastException($"Cannot cast DataContext to {nameof(ViewModelBase)}");
    }
}
