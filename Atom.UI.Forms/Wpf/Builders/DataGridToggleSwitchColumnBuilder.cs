using System.Windows.Controls;
using System.Windows.Data;
using MahApps.Metro.Controls;

namespace Genius.Atom.UI.Forms.Wpf.Builders;

internal sealed class DataGridToggleSwitchColumnBuilder : DataGridColumnBuilder
{
    internal DataGridToggleSwitchColumnBuilder(DataGridColumnBuilder parentBuilder)
        : base(parentBuilder.NotNull())
    {
    }

    public override DataGridTemplateColumn Build()
    {
        var column = CreateColumn();
        var binding = CreateBinding();
        binding.Mode = BindingMode.TwoWay;
        binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

        var elementFactory = new FrameworkElementFactory(typeof(ToggleSwitch));
        elementFactory.SetBinding(ToggleSwitch.IsOnProperty, binding);
        elementFactory.SetValue(ToggleSwitch.OnContentProperty, string.Empty);
        elementFactory.SetValue(ToggleSwitch.OffContentProperty, string.Empty);
        elementFactory.SetValue(FrameworkElement.MinWidthProperty, 22d);

        StylingHelpers.SetStyling(elementFactory, _cellStyling);

        var caption = Helpers.MakeCaptionFromPropertyName(_valuePath);
        elementFactory.SetValue(FrameworkElement.ToolTipProperty, caption);

        column.CellTemplate = new DataTemplate { VisualTree = elementFactory };

        return column;
    }
}
