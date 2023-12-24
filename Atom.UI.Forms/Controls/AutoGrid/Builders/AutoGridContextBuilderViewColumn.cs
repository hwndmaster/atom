using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderViewColumn<TViewModel, TParentViewModel> : IAutoGridContextBuilderColumn<IAutoGridContextBuilderViewColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>
{
    IAutoGridContextBuilderViewColumn<TViewModel, TParentViewModel> WithAttachedViewType(Type viewType);
}

internal sealed class AutoGridContextBuilderViewColumn<TViewModel, TParentViewModel> : AutoGridContextBuilderColumn<IAutoGridContextBuilderViewColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>, IAutoGridContextBuilderViewColumn<TViewModel, TParentViewModel>
{
    private Type? _viewType;

    public AutoGridContextBuilderViewColumn(PropertyDescriptor propertyDescriptor)
        : base(propertyDescriptor)
    {
    }

    public IAutoGridContextBuilderViewColumn<TViewModel, TParentViewModel> WithAttachedViewType(Type viewType)
    {
        _viewType = viewType;
        return this;
    }

    internal override AutoGridBuildColumnContext Build()
    {
        return new AutoGridBuildViewColumnContext(PropertyDescriptor, DetermineDisplayName(), _viewType.NotNull())
        {
            AutoWidth = _autoWidth,
            IsReadOnly = _isReadOnly,
            Style = _style
        };
    }

    protected override AutoGridContextBuilderViewColumn<TViewModel, TParentViewModel> BuilderInstance => this;
}
