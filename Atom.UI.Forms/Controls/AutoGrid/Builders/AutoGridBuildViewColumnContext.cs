using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal sealed class AutoGridBuildViewColumnContext : AutoGridBuildColumnContext
{
    public AutoGridBuildViewColumnContext(PropertyDescriptor property, string displayName, Type attachedViewType)
        : base(property, displayName)
    {
        AttachedViewType = attachedViewType;
    }

    public Type AttachedViewType { get; }
}
