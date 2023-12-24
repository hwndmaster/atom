using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

internal sealed class AutoGridColumnContext
{
    private readonly List<Action> _postProcessing = new();

    public AutoGridColumnContext(DataGrid dataGrid,
        DataGridAutoGeneratingColumnEventArgs args,
        AutoGridBuildColumnContext buildColumnContext)
    {
        DataGrid = dataGrid;
        Args = args;
        BuildColumn = buildColumnContext;
    }

    public DataGrid DataGrid { get; }
    public DataGridAutoGeneratingColumnEventArgs Args { get; }
    public AutoGridBuildColumnContext BuildColumn { get; }
    public AutoGridBuildCommandColumnContext? BuildCommandColumn => BuildColumn as AutoGridBuildCommandColumnContext;
    public AutoGridBuildToggleButtonColumnContext? BuildToggleButtonColumn => BuildColumn as AutoGridBuildToggleButtonColumnContext;
    public AutoGridBuildTextColumnContext? BuildTextColumn => BuildColumn as AutoGridBuildTextColumnContext;
    public AutoGridBuildViewColumnContext? BuildViewColumn => BuildColumn as AutoGridBuildViewColumnContext;

    public PropertyDescriptor Property => BuildColumn.Property;

    public bool IsReadOnly => DataGrid.IsReadOnly || Args.Column.IsReadOnly || BuildColumn.IsReadOnly;

    public Binding? GetBinding()
        => (Args.Column as DataGridBoundColumn)?.Binding as Binding;

    public void AddPostProcessing(Action action)
    {
        _postProcessing.Add(action);
    }

    public IEnumerable<Action> GetPostProcessingActions() => _postProcessing;
}
