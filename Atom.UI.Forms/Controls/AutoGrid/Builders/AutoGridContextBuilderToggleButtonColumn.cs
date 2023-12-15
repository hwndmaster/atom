using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderToggleButtonColumn : IAutoGridContextBuilderColumn<IAutoGridContextBuilderToggleButtonColumn>
{
    IAutoGridContextBuilderToggleButtonColumn WithIcons(string iconForTrue, string iconForFalse);
}

internal sealed class AutoGridContextBuilderToggleButtonColumn : AutoGridContextBuilderColumn<IAutoGridContextBuilderToggleButtonColumn>, IAutoGridContextBuilderToggleButtonColumn
{
    private string? _iconForTrue;
    private string? _iconForFalse;

    public AutoGridContextBuilderToggleButtonColumn(PropertyDescriptor propertyDescriptor)
        : base(propertyDescriptor)
    {
    }

    public IAutoGridContextBuilderToggleButtonColumn WithIcons(string iconForTrue, string iconForFalse)
    {
        _iconForTrue = iconForTrue;
        _iconForFalse = iconForFalse;
        return this;
    }

    public override AutoGridBuildColumnContext Build()
    {
        return new AutoGridBuildToggleButtonColumnContext(PropertyDescriptor, DetermineDisplayName())
        {
            AutoWidth = _autoWidth,
            IconForTrue = _iconForTrue,
            IconForFalse = _iconForFalse,
            IsReadOnly = _isReadOnly,
            Style = _style,
            ToolTip = _toolTip,
            ToolTipPath = _toolTipPath,
            ValueConverter = DetermineValueConverter(null),
            Visibility = _visibilityBinding,
        };
    }

    protected override AutoGridContextBuilderToggleButtonColumn BuilderInstance => this;
}
