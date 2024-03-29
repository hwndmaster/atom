using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;
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

    public static object GetItemsSource(DependencyObject element)
    {
        return element.GetValue(ItemsSourceProperty);
    }

    public static void SetItemsSource(DependencyObject element, object value)
    {
        element.SetValue(ItemsSourceProperty, value);
    }

    public static IAutoGridBuilder? GetAutoGridBuilder(DependencyObject element)
    {
        return (IAutoGridBuilder?)element.GetValue(AutoGridBuilderProperty);
    }

    public static void SetAutoGridBuilder(DependencyObject element, IAutoGridBuilder? value)
    {
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
        var buildContext = AutoGridBuildContext.CreateLazy(dataGrid).Value;

        if (buildContext.GroupByProperties.Length == 0 && buildContext.FilterByProperties.Length == 0)
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

            new GroupingBehavior().Attach(dataGrid, buildContext, collectionViewSource);
            new FilteringBehavior().Attach(dataGrid, buildContext, collectionViewSource);

            d.SetValue(DataGrid.ItemsSourceProperty, collectionViewSource.View);
        }

        new DynamicColumnsBehavior().Attach(dataGrid, buildContext);
    }
}
