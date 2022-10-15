using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public sealed class AutoGridBuildComboBoxColumnContext : AutoGridBuildColumnContext
{
    public AutoGridBuildComboBoxColumnContext(PropertyDescriptor property, string displayName,
        string collectionPropertyName, bool fromOwnerContext)
        : base(property, displayName)
    {
        CollectionPropertyName = collectionPropertyName.NotNull();
        FromOwnerContext = fromOwnerContext;
    }

    public string CollectionPropertyName { get; }
    public bool FromOwnerContext { get; }
}
