using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Genius.Atom.UI.Forms.Controls.AutoGrid.ColumnBehaviors;
using ReactiveUI;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

// TODO: Cover with unit tests
internal sealed class DynamicColumnsBehavior : IDisposable
{
    private readonly Disposer _disposer = new();
    private readonly DataGrid _dataGrid;
    private readonly AutoGridBuildContext _buildContext;
    private bool _dataGridHasGeneratedColumns;

    public DynamicColumnsBehavior(DataGrid dataGrid, AutoGridBuildContext buildContext)
    {
        _dataGrid = dataGrid.NotNull();
        _buildContext = buildContext.NotNull();

        _disposer.Add(DisposeCurrentState);
    }

    public DynamicColumnsBehavior Attach()
    {
        DisposeCurrentState();

        var dynamicColumnContexts = _buildContext.Columns.OfType<AutoGridBuildDynamicColumnContext>().ToArray();
        if (dynamicColumnContexts.Length == 0)
            return this;

        var vm = _dataGrid.GetViewModel();

        List<DynamicColumnContextState> states = [];
        foreach (var dynamicColumnContext in dynamicColumnContexts)
        {
            Disposer subscriptions = new();

            vm.WhenChanged<ViewModelBase, DynamicColumnsViewModel>(dynamicColumnContext.ColumnsPropertyName)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(dynamicColumnsVm =>
                {
                    HandleColumnsPropertyChange(dynamicColumnsVm, dynamicColumnContext);
                }).DisposeWith(subscriptions);

            var state = new DynamicColumnContextState
            {
                BuildContext = dynamicColumnContext,
                Subscriptions = subscriptions
            };

            Observable.FromEventPattern<EventHandler, EventArgs>(
                h => _dataGrid.AutoGeneratedColumns += h, h => _dataGrid.AutoGeneratedColumns -= h)
                .Subscribe(_ =>
                {
                    _dataGridHasGeneratedColumns = true;
                    PublishColumns(state);
                }).DisposeWith(subscriptions);

            states.Add(state);
        }

        _dataGrid.SetValue(Properties.DynamicColumnsProperty, states.ToArray());

        // Once the states are set up, do the initial call for all contexts if dynamic
        // columns are already available for some of them.
        foreach (var dynamicColumnContext in dynamicColumnContexts)
        {
            if (vm.TryGetPropertyValue(dynamicColumnContext.ColumnsPropertyName, out var columnsPropertyValue)
                && columnsPropertyValue is DynamicColumnsViewModel dynamicColumnsViewModel
                && dynamicColumnsViewModel.ColumnNames.Length > 0)
            {
                HandleColumnsPropertyChange(dynamicColumnsViewModel, dynamicColumnContext);
            }
        }

        return this;
    }

    public void Dispose()
    {
        _disposer.Dispose();
    }

    private void DisposeCurrentState()
    {
        var previousStates = _dataGrid.GetValue(Properties.DynamicColumnsProperty) as DynamicColumnContextState[];
        if (previousStates is not null)
        {
            foreach (var previousState in previousStates)
            {
                previousState.Subscriptions.Dispose();
                foreach (var previousColumn in previousState.Columns)
                    _dataGrid.Columns.Remove(previousColumn);
            }
        }
    }

    private void HandleColumnsPropertyChange(DynamicColumnsViewModel? dynamicColumnsVm, AutoGridBuildDynamicColumnContext dynamicColumnContext)
    {
        var states = _dataGrid.GetValue(Properties.DynamicColumnsProperty) as DynamicColumnContextState[]
                                ?? throw new InvalidOperationException("State object has not been initialized.");
        var thisContextState = states.First(x => x.BuildContext == dynamicColumnContext);

        _dataGrid.IsEnabled = false;
        CleanupState(thisContextState);

        List<DataGridTextColumn> dataGridColumns = [];
        if (dynamicColumnsVm is not null)
        {
            for (var i = 0; i < dynamicColumnsVm.ColumnNames.Length; i++)
            {
                DataGridTextColumn textColumn = new()
                {
                    Header = string.Empty,
                    Binding = new Binding($"{dynamicColumnContext.Property.Name}[{i}]")
                };
                var args = new DataGridAutoGeneratingColumnEventArgs(dynamicColumnsVm.ColumnNames[i], typeof(string), textColumn);
                dynamicColumnContext.DisplayName = dynamicColumnsVm.ColumnNames[i];
                var columnContext = new AutoGridColumnContext(_dataGrid, args, dynamicColumnContext);

                foreach (var columnBehavior in ColumnBehaviorsAccessor.GetForDynamicColumn())
                {
                    columnBehavior.Attach(columnContext);
                }

                dataGridColumns.Add(textColumn);
            }
        }

        _dataGrid.IsEnabled = true;

        thisContextState.Columns = [.. dataGridColumns];

        if (_dataGridHasGeneratedColumns)
        {
            PublishColumns(thisContextState);
        }
    }

    private void CleanupState(DynamicColumnContextState thisContextState)
    {
        foreach (var previousColumn in thisContextState.Columns)
        {
            _dataGrid.Columns.Remove(previousColumn);
        }
        thisContextState.Columns = [];
    }

    private void PublishColumns(DynamicColumnContextState state)
    {
        foreach (var dynamicColumn in state.Columns)
        {
            _dataGrid.Columns.Add(dynamicColumn);

            if (state.BuildContext.DisplayIndex is not null)
                dynamicColumn.DisplayIndex = Math.Min(_dataGrid.Columns.Count - 1, state.BuildContext.DisplayIndex.Value);
        }
    }
}
