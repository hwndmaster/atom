using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal sealed class AutoGridBuildCommandColumnContext : AutoGridBuildColumnContext
{
    public AutoGridBuildCommandColumnContext(PropertyDescriptor property, AutoGridContextBuilderBaseFields baseFields)
        : base(property, baseFields)
    {
    }

    public string? Icon { get; init; }
    public Size? IconSize { get; init; }
}
