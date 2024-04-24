using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal sealed class AutoGridBuildDynamicColumnContext : AutoGridBuildColumnContext
{
    public AutoGridBuildDynamicColumnContext(PropertyDescriptor property, AutoGridContextBuilderBaseFields baseFields)
        : base(property, baseFields)
    {
    }

    public required string ColumnsPropertyName { get; init; }
    public override bool IsAlwaysHidden => true;
}
