using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel> : IAutoGridContextBuilderColumn<IAutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>
{
    IAutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel> WithIcons(string iconForTrue, string iconForFalse);
}

internal sealed class AutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel> : AutoGridContextBuilderColumn<IAutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>, IAutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel>
{
    private string? _iconForTrue;
    private string? _iconForFalse;

    public AutoGridContextBuilderToggleButtonColumn(PropertyDescriptor propertyDescriptor)
        : base(propertyDescriptor)
    {
    }

    public IAutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel> WithIcons(string iconForTrue, string iconForFalse)
    {
        _iconForTrue = iconForTrue;
        _iconForFalse = iconForFalse;
        return this;
    }

    internal override AutoGridBuildColumnContext Build()
    {
        return new AutoGridBuildToggleButtonColumnContext(PropertyDescriptor, GetBaseFields())
        {
            IconForTrue = _iconForTrue,
            IconForFalse = _iconForFalse
        };
    }

    protected override AutoGridContextBuilderToggleButtonColumn<TViewModel, TParentViewModel> BuilderInstance => this;
}
