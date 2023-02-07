using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public sealed class AutoGridBuildCommandColumnContext : AutoGridBuildColumnContext
{
    public AutoGridBuildCommandColumnContext(PropertyDescriptor property, string displayName)
        : base(property, displayName)
    {
    }

    public string? Icon { get; init; }
    public Size? IconSize { get; init; }
}
