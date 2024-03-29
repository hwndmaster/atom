using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms;

public class DataGridSelectedItemsBehavior : Behavior<DataGrid>
{
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(INotifyCollectionChanged), typeof(DataGridSelectedItemsBehavior), new PropertyMetadata(OnSelectedItemsChanged));

    private IDisposable? _subscription;
    private bool _updateSuspended;

    public INotifyCollectionChanged SelectedItems
    {
        get { return (INotifyCollectionChanged)GetValue(SelectedItemsProperty); }
        set { SetValue(SelectedItemsProperty, value); }
    }

    protected override void OnAttached()
    {
        AssociatedObject.AddHandler(DataGridRow.UnselectedEvent, new RoutedEventHandler(OnDataGridRowUnselected), true);
        AssociatedObject.AddHandler(DataGridRow.SelectedEvent, new RoutedEventHandler(OnDataGridRowSelected), true);
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        _subscription?.Dispose();
        _subscription = null;

        AssociatedObject.RemoveHandler(DataGridRow.UnselectedEvent, new RoutedEventHandler(OnDataGridRowUnselected));
        AssociatedObject.RemoveHandler(DataGridRow.SelectedEvent, new RoutedEventHandler(OnDataGridRowSelected));
        base.OnDetaching();
    }

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (DataGridSelectedItemsBehavior)d;
        behavior.SubscribeToCollectionEvents();
    }

    private void SubscribeToCollectionEvents()
    {
        _subscription?.Dispose();
        _subscription = SelectedItems.WhenCollectionChanged()
            .Subscribe(args =>
            {
                if (_updateSuspended)
                {
                    return;
                }

                _updateSuspended = true;

                if (args.NewItems is not null)
                {
                    foreach (var item in args.NewItems)
                    {
                        AssociatedObject.SelectedItems.Add(item);
                    }
                }
                if (args.OldItems is not null)
                {
                    foreach (var item in args.OldItems)
                    {
                        AssociatedObject.SelectedItems.Remove(item);
                    }
                }
                if (args.Action == NotifyCollectionChangedAction.Reset)
                {
                    AssociatedObject.SelectedItems.Clear();
                }

                _updateSuspended = false;
            });
    }

    private void OnDataGridRowUnselected(object sender, RoutedEventArgs e)
    {
        if (_updateSuspended)
            return;

        var row = (DataGridRow)e.OriginalSource;
        var selectedItemsList = (IList)SelectedItems;
        _updateSuspended = true;
        selectedItemsList.Remove(row.DataContext);
        _updateSuspended = false;
    }

    private void OnDataGridRowSelected(object sender, RoutedEventArgs e)
    {
        if (_updateSuspended)
            return;

        var row = (DataGridRow)e.OriginalSource;
        var selectedItemsList = (IList)SelectedItems;
        _updateSuspended = true;
        selectedItemsList.Add(row.DataContext);
        _updateSuspended = false;
    }
}
