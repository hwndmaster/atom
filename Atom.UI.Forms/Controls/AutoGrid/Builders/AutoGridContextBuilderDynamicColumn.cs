using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel> : IAutoGridContextBuilderColumn<IAutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>
{
}

internal sealed class AutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel> : AutoGridContextBuilderColumn<IAutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>, IAutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel>
{
    private readonly string _columnsPropertyName;

    public AutoGridContextBuilderDynamicColumn(PropertyDescriptor entriesPropertyDescriptor, string columnsPropertyName)
        : base(entriesPropertyDescriptor)
    {
        _isReadOnly = true;
        _columnsPropertyName = columnsPropertyName;
    }

    public override IAutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel> IsReadOnly(bool isReadOnly = true)
    {
        if (!isReadOnly)
            throw new NotSupportedException("Only readonly dynamic columns are supported.");
        return this;
    }

    internal override AutoGridBuildColumnContext Build()
    {
        return new AutoGridBuildDynamicColumnContext(PropertyDescriptor, GetBaseFields(omitDisplayName: true))
        {
            ColumnsPropertyName = _columnsPropertyName
        };
    }

    protected override AutoGridContextBuilderDynamicColumn<TViewModel, TParentViewModel> BuilderInstance => this;
}
