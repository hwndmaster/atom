using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public sealed class AutoGridBuildToggleButtonColumnContext : AutoGridBuildColumnContext
{
    public AutoGridBuildToggleButtonColumnContext(PropertyDescriptor property, string displayName)
        : base(property, displayName)
    {
    }

    public string? IconForTrue { get; init; }
    public string? IconForFalse { get; init; }
}
