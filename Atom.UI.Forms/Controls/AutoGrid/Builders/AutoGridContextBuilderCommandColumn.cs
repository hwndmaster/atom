using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel> : IAutoGridContextBuilderColumn<IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>
{
    IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel> WithIcon(string icon, double size);
    IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel> WithIcon(string icon, Size? size = null);
}

internal sealed class AutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel> : AutoGridContextBuilderColumn<IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>, IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel>
{
    private string? _icon;
    private Size? _iconSize;

    public AutoGridContextBuilderCommandColumn(PropertyDescriptor propertyDescriptor)
        : base(propertyDescriptor)
    {
    }

    public IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel> WithIcon(string icon, double size)
    {
        return WithIcon(icon, new Size(size, size));
    }

    public IAutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel> WithIcon(string icon, Size? size = null)
    {
        _icon = icon;
        _iconSize = size;
        return this;
    }

    internal override AutoGridBuildColumnContext Build()
    {
        return new AutoGridBuildCommandColumnContext(PropertyDescriptor, DetermineDisplayName())
        {
            AutoWidth = _autoWidth,
            Icon = _icon,
            IconSize = _iconSize,
            IsReadOnly = _isReadOnly,
            Style = _style,
            ToolTip = _toolTip,
            ToolTipPath = _toolTipPath,
            ValueConverter = DetermineValueConverter(null),
            Visibility = _visibilityBinding,
        };
    }

    protected override AutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel> BuilderInstance => this;
}
