using System.Collections;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

// TODO: Cover with unit tests
internal sealed class GroupingBehavior
{
    public void Attach(DataGrid dataGrid, AutoGridBuildContext buildContext, CollectionViewSource collectionViewSource)
    {
        if (buildContext.GroupByProperties.Length == 0)
            return;

        if (collectionViewSource.Source is IEnumerable enumerable)
        {
            // Attach current items
            AttachToPropertyChangedEvents(buildContext, collectionViewSource, enumerable);

            // Ensure all new items will be attached
            var observableCollection = collectionViewSource.Source as ITypedObservableCollection;
            if (observableCollection is not null)
            {
                // TODO: Dispose event subscription when detached
                // TODO: Use ObservableExtensions.WhenCollectionChanged
                observableCollection.CollectionChanged += (sender, args) => {
                    if (args.Action == NotifyCollectionChangedAction.Add)
                    {
                        AttachToPropertyChangedEvents(buildContext, collectionViewSource, args.NewItems!);
                    }
                };
            }
        }

        foreach (var groupByProp in buildContext.GroupByProperties)
        {
            collectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(groupByProp.Property.Name));
        }

        if (buildContext.GroupByProperties.Any(x => AutoGridBuilderHelpers.IsGroupableColumn(x.Property)))
        {
            dataGrid.GroupStyle.Add((GroupStyle)Application.Current.FindResource("Atom.AutoGrid.Group.GroupableViewModel"));
            dataGrid.SetValue(Grid.IsSharedSizeScopeProperty, true);
        }
        else
        {
            dataGrid.GroupStyle.Add((GroupStyle)Application.Current.FindResource("Atom.AutoGrid.Group.String"));
        }
    }

    private static void AttachToPropertyChangedEvents(AutoGridBuildContext buildContext, CollectionViewSource collectionViewSource, IEnumerable items)
    {
        foreach (var childViewModel in items.OfType<ViewModelBase>())
        {
            foreach (var groupByProp in buildContext.GroupByProperties)
            {
                childViewModel.WhenChanged(groupByProp.Property.Name, (object _) =>
                    collectionViewSource.View.Refresh());
            }
        }
    }
}
