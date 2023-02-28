using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using Genius.Atom.UI.Forms.Wpf;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms;

// NOTE: This implementation was originally taken here:
//       https://github.com/markjulmar/mvvmhelpers/blob/master/Julmar.Wpf.Helpers/Julmar.Wpf.Behaviors/Interactivity/MultiSelectTreeViewBehavior.cs
// UPD 2022-Dec-28: Added binding between ObservableCollection<> and IsSelected property.
// UPD 2022-Dec-28: Handling item removal from the TreeView.

/// <summary>
///   Behavior to support multi-select in a traditional WPF TreeView control.
/// </summary>
/// <example>
/// <![CDATA[
///   <TreeView ...>
///      <i:Interaction.Behaviors>
///         <b:TreeViewMultiSelectBehavior SelectedItems="{Binding SelectedItems}" />
///      </i:Interaction.Behaviors>
///   </TreeView>
/// ]]>
/// </example>
public class TreeViewMultiSelectBehavior : Behavior<TreeView>
{
    private TreeViewItem? _anchorItem;
    private IDisposable? _subscription;
    private bool _updateSuspended = false;

    /// <summary>
    ///   Selected Items collection.
    /// </summary>
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(TreeViewMultiSelectBehavior),
        new PropertyMetadata(OnSelectedItemsPropertyChanged));

    /// <summary>
    ///   Selected Items collection (intended to be data bound).
    /// </summary>
    public IList SelectedItems
    {
        get { return (IList)GetValue(SelectedItemsProperty); }
        set { SetValue(SelectedItemsProperty, value); }
    }

    /// <summary>
    ///   Selection attached property - can be used for styling TreeViewItem elements.
    /// </summary>
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.RegisterAttached("IsSelected", typeof (bool), typeof(TreeViewMultiSelectBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedChanged));

    /// <summary>
    ///   Returns whether the TreeViewItem is selected.
    /// </summary>
    public static bool GetIsSelected(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsSelectedProperty);
    }

    /// <summary>
    ///   Changes the selection state of the TreeViewItem.
    /// </summary>
    public static void SetIsSelected(DependencyObject obj, bool value)
    {
        obj.SetValue(IsSelectedProperty, value);

        if (!value && (bool)obj.GetValue(TreeViewItem.IsSelectedProperty))
        {
            obj.SetValue(TreeViewItem.IsSelectedProperty, false);
        }
    }

    /// <summary>
    ///   Called after the behavior is attached to an AssociatedObject.
    /// </summary>
    /// <remarks>
    ///   Override this to hook up functionality to the AssociatedObject.
    /// </remarks>
    protected override void OnAttached()
    {
        AssociatedObject.AddHandler(TreeViewItem.UnselectedEvent, new RoutedEventHandler(OnTreeViewItemUnselected), true);
        AssociatedObject.AddHandler(TreeViewItem.SelectedEvent, new RoutedEventHandler(OnTreeViewItemSelected), true);
        AssociatedObject.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnKeyDown), true);
        base.OnAttached();
    }

    /// <summary>
    ///   Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
    /// </summary>
    /// <remarks>
    ///   Override this to unhook functionality from the AssociatedObject.
    /// </remarks>
    protected override void OnDetaching()
    {
        AssociatedObject.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnKeyDown));
        AssociatedObject.RemoveHandler(TreeViewItem.UnselectedEvent, new RoutedEventHandler(OnTreeViewItemUnselected));
        AssociatedObject.RemoveHandler(TreeViewItem.SelectedEvent, new RoutedEventHandler(OnTreeViewItemSelected));

        _subscription?.Dispose();

        base.OnDetaching();
    }

    /// <summary>
    ///   Is called when the a tree item is unselected.
    /// </summary>
    private void OnTreeViewItemUnselected(object sender, RoutedEventArgs e)
    {
        if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == ModifierKeys.None)
        {
            SetIsSelected((TreeViewItem)e.OriginalSource, false);
        }
    }

    /// <summary>
    ///   Is called when the tree item is selected.
    /// </summary>
    private void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
    {
        TreeViewItem item = (TreeViewItem) e.OriginalSource;

        // Look for a disconnected item.  We can get this if the data source changes underneath us,
        // in which case we want to ignore this selection. This is actually a bug in WPF4, see:
        // http://connect.microsoft.com/VisualStudio/feedback/details/619658/wpf-virtualized-control-disconnecteditem-reference-when-datacontext-switch
        // Unfortunately, there's no way to reliably see this so we just look for the magic string here.
        // in WPF 4.5 they have a new static property which exposes this object off the BindingExpression.
        //
        // Could also check against this object, but not any safer than the string really.
        //var disconnectedItemSingleton = typeof(System.Windows.Data.BindingExpressionBase).GetField("DisconnectedItem", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        if (item.DataContext?.ToString() == "{DisconnectedItem}")
            return;

        if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) !=
            (ModifierKeys.Shift | ModifierKeys.Control))
        {
            switch ((Keyboard.Modifiers & ModifierKeys.Control))
            {
                case ModifierKeys.Control:
                    ToggleSelect(item);
                    break;
                default:
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                        AnchorMultiSelect(item);
                    else
                        SingleSelect(item);
                    break;
            }
        }
    }

    private static void OnSelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
        var behavior = (TreeViewMultiSelectBehavior)d;
        behavior.SubscribeToCollectionEvents();
    }

    private void SubscribeToCollectionEvents()
    {
        if (SelectedItems is not INotifyCollectionChanged observableCollection)
        {
            return;
        }

        _subscription?.Dispose();
        _subscription = observableCollection.WhenCollectionChanged()
            .Subscribe(args =>
            {
                if (_updateSuspended)
                {
                    return;
                }

                _updateSuspended = true;

                var treeViewItems = GetExpandedTreeViewItems().ToList();

                SelectItems(args.NewItems, true);
                SelectItems(args.OldItems, false);

                if (args.Action == NotifyCollectionChangedAction.Reset)
                {
                    foreach (var treeViewItem in treeViewItems)
                    {
                        SetIsSelected(treeViewItem, false);
                    }
                }

                _updateSuspended = false;

                void SelectItems(IList? items, bool isSelected)
                {
                    if (items is null) return;

                    foreach (var item in items)
                    {
                        var treeViewItem = treeViewItems!.Find(x => x.DataContext == item);
                        if (treeViewItem is not null)
                        {
                            SetIsSelected(treeViewItem, isSelected);
                        }
                    }
                }
            });
    }

    /// <summary>
    ///   Locates the <see cref="TreeView"/> parent for a given <see cref="TreeViewItem"/>.
    /// </summary>
    /// <param name="item">A tree view item.</param>
    /// <returns>The tree view control containing given item.</returns>
    private static TreeView GetTree(TreeViewItem item)
    {
        return item.FindVisualParent<TreeView>().NotNull();
    }

    private static void OnSelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        TreeViewItem item = (TreeViewItem)sender;
        TreeView tree = GetTree(item);
        Debug.Assert(tree is not null);

        var behavior = Interaction.GetBehaviors(tree).OfType<TreeViewMultiSelectBehavior>().SingleOrDefault();
        if (behavior is null || behavior.SelectedItems is null)
        {
            return;
        }

        var isSelected = GetIsSelected(item);
        var value = item.DataContext ?? item;

        if (!behavior._updateSuspended)
        {
            behavior._updateSuspended = true;
            if (isSelected)
            {
                behavior.SelectedItems.Add(value);
            }
            else
            {
                behavior.SelectedItems.Remove(value);
            }
            behavior._updateSuspended = false;
        }

        if (value is ISelectable selectableValue)
        {
            selectableValue.IsSelected = isSelected;
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        TreeView tree = (TreeView)sender;
        Debug.Assert(tree == AssociatedObject);

        // If you press CTRL+A, do a full select.
        if (e.Key == Key.A && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
        {
            GetExpandedTreeViewItems(tree)
                .ToList()
                .ForEach(tvi => SetIsSelected(tvi, true));

            e.Handled = true;
        }
    }

    /// <summary>
    ///   Returns the entire TreeView set of nodes.  Unfortunately, in WPF the TreeView
    ///   doesn't manage a selection state globally - it's singular, and compartmentalized into
    ///   each ItemsControl expansion. This is a heavy-handed approach, but for reasonably sized
    ///   tree views it should be ok.
    /// </summary>
    private IEnumerable<TreeViewItem> GetExpandedTreeViewItems(ItemsControl? container = null)
    {
        container ??= AssociatedObject;

        for (int i = 0; i < container.Items.Count; i++)
        {
            var item = container.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
            if (item is null)
                continue;

            // Hand back this child
            yield return item;

            // Hand back all the children
            foreach (var subItem in GetExpandedTreeViewItems(item))
                yield return subItem;
        }
    }

    /// <summary>
    ///   Is used to perform a multi-select operation using an anchor position.
    /// </summary>
    private void AnchorMultiSelect(TreeViewItem newItem)
    {
        if (_anchorItem is null)
        {
            var selectedItems = GetExpandedTreeViewItems().Where(GetIsSelected).ToList();
            _anchorItem = selectedItems.Count > 0
                ? selectedItems[^1]
                : GetExpandedTreeViewItems().FirstOrDefault();
            if (_anchorItem is null)
                return;
        }

        var anchor = _anchorItem;
        var items = GetExpandedTreeViewItems();
        bool inSelectionRange = false;

        foreach (var item in items)
        {
            bool isEdge = item == anchor || item == newItem;
            if (isEdge)
                inSelectionRange = !inSelectionRange;

            SetIsSelected(item, inSelectionRange || isEdge);
        }
    }

    /// <summary>
    ///   Performs a single-select operation.
    /// </summary>
    private void SingleSelect(TreeViewItem item)
    {
        foreach (TreeViewItem selectedItem in GetExpandedTreeViewItems())
            SetIsSelected(selectedItem, selectedItem == item);

        while (SelectedItems.Count > 1)
        {
            // Handle item removed
            SelectedItems.RemoveAt(0);
        }

        _anchorItem = item;
    }

    private void ToggleSelect(TreeViewItem item)
    {
        SetIsSelected(item, !GetIsSelected(item));
        _anchorItem ??= item;
    }
}
