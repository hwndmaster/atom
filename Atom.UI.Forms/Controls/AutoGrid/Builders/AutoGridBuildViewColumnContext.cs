using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal sealed class AutoGridBuildViewColumnContext : AutoGridBuildColumnContext
{
    public AutoGridBuildViewColumnContext(PropertyDescriptor property, AutoGridContextBuilderBaseFields baseFields)
        : base(property, baseFields)
    {
    }

    public required Type AttachedViewType { get; init; }
}
