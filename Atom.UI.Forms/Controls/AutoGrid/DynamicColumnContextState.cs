using System.Windows.Controls;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

internal sealed class DynamicColumnContextState
{
    public required AutoGridBuildDynamicColumnContext BuildContext { get; init; }
    public required IDisposable Subscriptions { get; init; }
    public DataGridTextColumn[] Columns { get; set; } = Array.Empty<DataGridTextColumn>();
}
