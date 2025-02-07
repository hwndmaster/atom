using System.Collections.Immutable;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Genius.Atom.UI.Forms.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

public static class Properties
{
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached(
        "ItemsSource",
        typeof(object),
        typeof(Properties),
        new PropertyMetadata(ItemsSourceChanged));

    public static readonly DependencyProperty DynamicColumnsProperty = DependencyProperty.RegisterAttached(
        "DynamicColumns",
        typeof(DynamicColumnContextState[]),
        typeof(Properties));

    public static readonly DependencyProperty AutoGridBuilderProperty = DependencyProperty.RegisterAttached(
        "AutoGridBuilder",
        typeof(IAutoGridBuilder),
        typeof(Properties),
        new PropertyMetadata());

    public static readonly DependencyProperty BuildContextProperty = DependencyProperty.RegisterAttached(
        "BuildContext",
        typeof(AutoGridBuildContext),
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

    public static readonly DependencyProperty SortedColumnsProperty = DependencyProperty.RegisterAttached(
        "SortedColumns",
        typeof(ImmutableList<ColumnSortingInfo>),
        typeof(Properties),
        new PropertyMetadata(OnSortedColumnsChanged));

    public static object GetItemsSource(DependencyObject element)
    {
        Guard.NotNull(element);
        return element.GetValue(ItemsSourceProperty);
    }

    public static void SetItemsSource(DependencyObject element, object value)
    {
        Guard.NotNull(element);
        element.SetValue(ItemsSourceProperty, value);
    }

    public static IAutoGridBuilder? GetAutoGridBuilder(DependencyObject element)
    {
        Guard.NotNull(element);
        return (IAutoGridBuilder?)element.GetValue(AutoGridBuilderProperty);
    }

    public static void SetAutoGridBuilder(DependencyObject element, IAutoGridBuilder? value)
    {
        Guard.NotNull(element);
        element.SetValue(AutoGridBuilderProperty, value);
    }

    internal static AutoGridBuildContext? GetBuildContext(DependencyObject element)
    {
        return (AutoGridBuildContext?)element.GetValue(BuildContextProperty);
    }

    internal static void SetBuildContext(DependencyObject element, AutoGridBuildContext? value)
    {
        element.SetValue(BuildContextProperty, value);
    }

    public static object GetSortedColumns(DependencyObject element)
    {
        Guard.NotNull(element);
        return element.GetValue(SortedColumnsProperty);
    }

    public static void SetSortedColumns(DependencyObject element, object value)
    {
        Guard.NotNull(element);
        element.SetValue(SortedColumnsProperty, value);
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

        const int Column = 0;
        if (presenter.ItemContainerGenerator.ContainerFromIndex(Column) is not DataGridCell cell)
            return;

        var grid = row.FindVisualParent<DataGrid>();
        if (grid is null)
            return;

        if ((bool)e.NewValue)
        {
            grid.ScrollIntoView(row, cell.Column);
            cell.Focus();

            grid.CurrentCell = new DataGridCellInfo(cell);

            var dispatcher = Module.ServiceProvider.GetRequiredService<IUiDispatcher>();
            dispatcher.Invoke(() => grid.BeginEdit());
        }
        else
        {
            grid.EndInit();
        }
    }

    private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dataGrid = (DataGrid)d;
        var collectionViewSource = e.NewValue is CollectionViewSource cvs
            ? cvs
            : new CollectionViewSource
            {
                Source = e.NewValue
            };

        AttachingBehavior.FindMeFor(dataGrid)
            .SetupItemsSource(collectionViewSource);
    }

    private static void OnSortedColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dataGrid = (DataGrid)d;
        if (!dataGrid.CanUserSortColumns)
            return;

        if (AttachingBehavior.FindMeFor(dataGrid).IsSortedColumnsUpdateSuspended)
        {
            return;
        }

        var columnsSortingInfo = e.NewValue as ImmutableList<ColumnSortingInfo>;
        if (columnsSortingInfo is null)
            return;

        var itemsSource = dataGrid.GetValue(DataGrid.ItemsSourceProperty);
        if (itemsSource is not ICollectionView collectionView)
            return;

        collectionView.SortDescriptions.Clear();
        foreach (var sortingInfo in columnsSortingInfo)
        {
            collectionView.SortDescriptions.Add(new SortDescription(sortingInfo.ColumnName,
                sortingInfo.SortAsc ? ListSortDirection.Ascending : ListSortDirection.Descending));
        }
    }
}
