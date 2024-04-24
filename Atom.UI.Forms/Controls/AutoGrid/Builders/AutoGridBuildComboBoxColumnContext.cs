using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal sealed class AutoGridBuildComboBoxColumnContext : AutoGridBuildColumnContext
{
    public AutoGridBuildComboBoxColumnContext(PropertyDescriptor property, AutoGridContextBuilderBaseFields baseFields)
        : base(property, baseFields)
    {
    }

    public required string CollectionPropertyName { get; init; }
    public required bool FromOwnerContext { get; init; }
}
