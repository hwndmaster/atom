using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

public sealed class AutoGridColumnContext
{
    private readonly List<Action> _postProcessing = new();

    public AutoGridColumnContext(DataGrid dataGrid, DataGridAutoGeneratingColumnEventArgs args,
        PropertyDescriptor property)
    {
        DataGrid = dataGrid;
        Args = args;
        Property = property;
    }

    public DataGrid DataGrid { get; }
    public DataGridAutoGeneratingColumnEventArgs Args { get; }
    public PropertyDescriptor Property { get; }

    public T? GetAttribute<T>() where T: Attribute
        => Property.Attributes.OfType<T>().FirstOrDefault();

    public Binding? GetBinding()
        => (Args.Column as DataGridBoundColumn)?.Binding as Binding;

    public void AddPostProcessing(Action action)
    {
        _postProcessing.Add(action);
    }

    public IEnumerable<Action> GetPostProcessingActions() => _postProcessing;
}
