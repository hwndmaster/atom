using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderViewColumn : IAutoGridContextBuilderColumn<IAutoGridContextBuilderViewColumn>
{
    IAutoGridContextBuilderViewColumn WithAttachedViewType(Type viewType);
}

internal sealed class AutoGridContextBuilderViewColumn : AutoGridContextBuilderColumn<IAutoGridContextBuilderViewColumn>, IAutoGridContextBuilderViewColumn
{
    private Type? _viewType;

    public AutoGridContextBuilderViewColumn(PropertyDescriptor propertyDescriptor)
        : base(propertyDescriptor)
    {
    }

    public IAutoGridContextBuilderViewColumn WithAttachedViewType(Type viewType)
    {
        _viewType = viewType;
        return this;
    }

    public override AutoGridBuildColumnContext Build()
    {
        return new AutoGridBuildViewColumnContext(PropertyDescriptor, DetermineDisplayName(), _viewType.NotNull())
        {
            AutoWidth = _autoWidth,
            IsReadOnly = _isReadOnly,
            Style = _style
        };
    }

    protected override AutoGridContextBuilderViewColumn BuilderInstance => this;
}
