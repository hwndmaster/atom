using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal sealed class AutoGridBuildDynamicColumnContext : AutoGridBuildColumnContext
{
    public AutoGridBuildDynamicColumnContext(PropertyDescriptor property)
        : base(property)
    {
    }

    public required string ColumnsPropertyName { get; init; }
}
