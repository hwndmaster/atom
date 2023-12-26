using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using ReactiveUI;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

// TODO: Cover with unit tests
internal sealed class DynamicColumnsBehavior
{
    public void Attach(DataGrid dataGrid, AutoGridBuildContext buildContext)
    {
        var previousStates = dataGrid.GetValue(Properties.DynamicColumnsProperty) as DynamicColumnContextState[];
        if (previousStates is not null)
        {
            foreach (var previousState in previousStates)
            {
                previousState.Subscription.Dispose();
                foreach (var previousColumn in previousState.Columns)
                {
                    dataGrid.Columns.Remove(previousColumn);
                }
            }
        }

        var dynamicColumnContexts = buildContext.Columns.OfType<AutoGridBuildDynamicColumnContext>().ToArray();
        if (!dynamicColumnContexts.Any())
            return;

        var vm = GetViewModel(dataGrid);

        List<DynamicColumnContextState> states = new();
        foreach (var dynamicColumnContext in dynamicColumnContexts)
        {
            var subscription = vm
                .WhenChanged<ViewModelBase, DynamicColumnsViewModel>(dynamicColumnContext.ColumnsPropertyName)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(dynamicColumnsVm =>
                {
                    HandleColumnsPropertyChange(dataGrid, dynamicColumnsVm, dynamicColumnContext);
                });


            states.Add(new DynamicColumnContextState
            {
                BuildContext = dynamicColumnContext,
                Subscription = subscription
            });
        }

        dataGrid.SetValue(Properties.DynamicColumnsProperty, states.ToArray());

        // Once the states are set up, do the initial call for all contexts if dynamic
        // columns are already available for some of them.
        foreach (var dynamicColumnContext in dynamicColumnContexts)
        {
            if (vm.TryGetPropertyValue(dynamicColumnContext.ColumnsPropertyName, out var columnsPropertyValue)
                && columnsPropertyValue is DynamicColumnsViewModel dynamicColumnsViewModel
                && dynamicColumnsViewModel.ColumnNames.Length > 0)
            {
                HandleColumnsPropertyChange(dataGrid, dynamicColumnsViewModel, dynamicColumnContext);
            }
        }
    }

    private static void HandleColumnsPropertyChange(DataGrid dataGrid, DynamicColumnsViewModel? dynamicColumnsVm, AutoGridBuildDynamicColumnContext dynamicColumnContext)
    {
        var states = dataGrid.GetValue(Properties.DynamicColumnsProperty) as DynamicColumnContextState[]
                                ?? throw new InvalidOperationException("State object has not been initialized.");
        var thisContextState = states.First(x => x.BuildContext == dynamicColumnContext);

        foreach (var previousColumn in thisContextState.Columns)
        {
            dataGrid.Columns.Remove(previousColumn);
        }

        List<DataGridTextColumn> dataGridColumns = new();
        if (dynamicColumnsVm is not null)
        {
            for (var i = 0; i < dynamicColumnsVm.ColumnNames.Length; i++)
            {
                DataGridTextColumn textColumn = new()
                {
                    Header = dynamicColumnsVm.ColumnNames[i],
                    Binding = new Binding($"{dynamicColumnContext.Property.Name}[{i}]")
                };
                dataGrid.Columns.Add(textColumn);
                dataGridColumns.Add(textColumn);
            }
        }

        thisContextState.Columns = dataGridColumns.ToArray();
    }

    private static ViewModelBase GetViewModel(DataGrid dataGrid)
    {
        return dataGrid.DataContext as ViewModelBase
            ?? throw new InvalidCastException($"Cannot cast DataContext to {nameof(ViewModelBase)}");
    }
}
