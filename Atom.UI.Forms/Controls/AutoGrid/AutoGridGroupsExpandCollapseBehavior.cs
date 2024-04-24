using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Wpf;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

internal sealed class AutoGridGroupsExpandCollapseBehavior : Behavior<MenuItem>
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        "Command", typeof(string), typeof(AutoGridGroupsExpandCollapseBehavior), new PropertyMetadata(default(string)));

    public string Command
    {
        get { return (string)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    protected override void OnAttached()
    {
        AssociatedObject.Click += MenuItem_Click;

        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Click -= MenuItem_Click;
        base.OnDetaching();
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender;
        var contextMenu = menuItem.Parent as ContextMenu
            ?? throw new InvalidOperationException($"Wrong parent type. Expected: {nameof(ContextMenu)}.");
        var groupItem = contextMenu.TemplatedParent as GroupItem
            ?? throw new InvalidOperationException($"Wrong templated parent type. Expected: {nameof(GroupItem)}.");
        var dataGrid = groupItem.FindVisualParent<DataGrid>()
            ?? throw new InvalidOperationException("Couldn't find the DataGrid associated to this menu item.");

        var itemsSource = dataGrid.ItemsSource as ICollectionView
            ?? throw new InvalidOperationException($"Only {nameof(DataGrid)} with {nameof(DataGrid.ItemsSource)} set to {nameof(ICollectionView)} is supported.");

        var groups = itemsSource.Groups.Cast<CollectionViewGroup>();

        using (var _ = itemsSource.DeferRefresh())
        {
            switch (Command)
            {
                case "ExpandAll":
                {
                    foreach (var group in groups)
                        ((IGroupableViewModel)group.Name).IsExpanded = true;
                    break;
                }
                case "CollapseAll":
                {
                    foreach (var group in groups)
                        ((IGroupableViewModel)group.Name).IsExpanded = false;
                    break;
                }
                case "CollapseAllButThis":
                {
                    var thisGroup = (CollectionViewGroup)menuItem.DataContext;
                    foreach (var group in groups)
                        ((IGroupableViewModel)group.Name).IsExpanded = group == thisGroup;
                    break;
                }
                default:
                    throw new ArgumentException("No associated command found for: " + Command);
            }
        }

        itemsSource.Refresh();
    }
}
