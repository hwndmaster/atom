using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel> : IAutoGridContextBuilderColumn<IAutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>
{
    IAutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel> WithListSource(string collectionPropertyPath, bool fromOwnerContext);
}

internal sealed class AutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel> : AutoGridContextBuilderColumn<IAutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>, IAutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel>
{
    private string? _collectionPropertyPath;
    private bool _fromOwnerContext;

    public AutoGridContextBuilderComboBoxColumn(PropertyDescriptor propertyDescriptor)
        : base(propertyDescriptor)
    {
    }

    public IAutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel> WithListSource(string collectionPropertyPath, bool fromOwnerContext)
    {
        _collectionPropertyPath = collectionPropertyPath.NotNull();
        _fromOwnerContext = fromOwnerContext;
        return this;
    }

    internal override AutoGridBuildColumnContext Build()
    {
        Guard.NotNull(_collectionPropertyPath, message: "Call .WithListSource() before calling Build().");

        return new AutoGridBuildComboBoxColumnContext(PropertyDescriptor, GetBaseFields())
        {
            CollectionPropertyName = _collectionPropertyPath,
            FromOwnerContext = _fromOwnerContext
        };
    }

    protected override AutoGridContextBuilderComboBoxColumn<TViewModel, TParentViewModel> BuilderInstance => this;
}
