using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderCommandColumn : IAutoGridContextBuilderColumn<IAutoGridContextBuilderCommandColumn>
{
    IAutoGridContextBuilderCommandColumn WithIcon(string icon);
}

internal sealed class AutoGridContextBuilderCommandColumn : AutoGridContextBuilderColumn<IAutoGridContextBuilderCommandColumn>, IAutoGridContextBuilderCommandColumn
{
    private string? _icon;

    public AutoGridContextBuilderCommandColumn(PropertyDescriptor propertyDescriptor)
        : base(propertyDescriptor)
    {
    }

    public IAutoGridContextBuilderCommandColumn WithIcon(string icon)
    {
        _icon = icon;
        return this;
    }

    public override AutoGridBuildColumnContext Build()
    {
        return new AutoGridBuildCommandColumnContext(PropertyDescriptor, DetermineDisplayName())
        {
            AutoWidth = _autoWidth,
            Icon = _icon,
            IsReadOnly = _isReadOnly,
            Style = _style,
            ToolTipPath = _toolTipPath,
            ValueConverter = DetermineValueConverter(null)
        };
    }

    protected override AutoGridContextBuilderCommandColumn BuilderInstance => this;
}
