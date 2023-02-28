using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms;

public sealed class ListBoxSelectedItemsBehavior : Behavior<ListBox>
{
    public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
        nameof(SelectedItems),
        typeof(IList),
        typeof(ListBoxSelectedItemsBehavior),
        new PropertyMetadata(OnSelectedItemsPropertyChanged));

    private CompositeDisposable _subscriptions = new();
    private bool _isUpdating = false;

    protected override void OnAttached()
    {
        AssociatedObject.SelectionChanged += OnListBoxSelectionChanged;

        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.SelectionChanged -= OnListBoxSelectionChanged;

        _subscriptions.Dispose();
        _subscriptions = new();

        base.OnDetaching();
    }

    private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdating)
            return;

        foreach (var item in e.AddedItems)
        {
            SelectedItems.Add(item);
        }

        foreach (var item in e.RemovedItems)
        {
            SelectedItems.Remove(item);
        }
    }

    private static void OnSelectedItemsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (args.NewValue is not IList list)
        {
            return;
        }

        var behavior = (ListBoxSelectedItemsBehavior)obj;

        behavior._isUpdating = true;
        try
        {
            behavior.AssociatedObject.SelectedItems.Clear();
            foreach (var item in list)
            {
                behavior.AssociatedObject.SelectedItems.Add(item);
            }
        }
        finally
        {
            behavior._isUpdating = false;
        }

        if (list is INotifyCollectionChanged observableCollection)
        {
            behavior.AttachToObservableCollection(observableCollection);
        }
    }

    private void AttachToObservableCollection(INotifyCollectionChanged observableCollection)
    {
        _subscriptions.Add(observableCollection.WhenCollectionChanged()
            .Subscribe(args =>
            {
                if (_isUpdating)
                {
                    return;
                }

                _isUpdating = true;

                if (args.NewItems is not null)
                {
                    foreach (var item in args.NewItems)
                    {
                        AssociatedObject.SelectedItems.Add(item);
                    }
                }

                if (args.OldItems is not null && args.Action == NotifyCollectionChangedAction.Remove)
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

                _isUpdating = false;
            }));
    }

    public IList SelectedItems
    {
        get { return (IList)GetValue(SelectedItemsProperty); }
        set { SetValue(SelectedItemsProperty, value); }
    }
}
