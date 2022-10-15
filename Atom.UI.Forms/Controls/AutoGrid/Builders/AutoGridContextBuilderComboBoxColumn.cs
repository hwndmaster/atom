using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderComboBoxColumn : IAutoGridContextBuilderColumn<IAutoGridContextBuilderComboBoxColumn>
{
    IAutoGridContextBuilderComboBoxColumn WithListSource(string collectionPropertyPath, bool fromOwnerContext);
}

internal sealed class AutoGridContextBuilderComboBoxColumn : AutoGridContextBuilderColumn<IAutoGridContextBuilderComboBoxColumn>, IAutoGridContextBuilderComboBoxColumn
{
    private string? _collectionPropertyName;
    private bool _fromOwnerContext;

    public AutoGridContextBuilderComboBoxColumn(PropertyDescriptor propertyDescriptor)
        : base(propertyDescriptor)
    {
    }

    public IAutoGridContextBuilderComboBoxColumn WithListSource(string collectionPropertyName, bool fromOwnerContext)
    {
        _collectionPropertyName = collectionPropertyName.NotNull();
        _fromOwnerContext = fromOwnerContext;
        return this;
    }

    public override AutoGridBuildColumnContext Build()
    {
        Guard.NotNull(_collectionPropertyName, message: "Call .WithListSource() before calling Build().");

        return new AutoGridBuildComboBoxColumnContext(PropertyDescriptor, DetermineDisplayName(),
            _collectionPropertyName, _fromOwnerContext)
        {
            AutoWidth = _autoWidth,
            IsReadOnly = _isReadOnly,
            Style = _style
        };
    }

    protected override AutoGridContextBuilderComboBoxColumn BuilderInstance => this;
}
