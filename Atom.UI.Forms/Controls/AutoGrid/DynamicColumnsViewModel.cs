namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

public sealed class DynamicColumnsViewModel : ViewModelBase
{
    public DynamicColumnsViewModel(string[] columnNames)
    {
        ColumnNames = columnNames.NotNull();
    }

    public string[] ColumnNames { get; }
}
