using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal sealed class AutoGridBuildToggleButtonColumnContext : AutoGridBuildColumnContext
{
    public AutoGridBuildToggleButtonColumnContext(PropertyDescriptor property, AutoGridContextBuilderBaseFields baseFields)
        : base(property, baseFields)
    {
    }

    public string? IconForTrue { get; init; }
    public string? IconForFalse { get; init; }
}
