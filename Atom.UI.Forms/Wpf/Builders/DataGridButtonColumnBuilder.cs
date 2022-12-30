using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Wpf.Builders;

internal sealed class DataGridButtonColumnBuilder : DataGridColumnBuilder
{
    private readonly string? _imagePath;

    internal DataGridButtonColumnBuilder(DataGridColumnBuilder parentBuilder, string? imagePath)
        : base(parentBuilder.NotNull())
    {
        _imagePath = imagePath;
    }

    public override DataGridTemplateColumn Build()
    {
        var column = CreateColumn();
        var caption = Helpers.MakeCaptionFromPropertyName(_valuePath.Replace("Command", ""));

        var buttonFactory = new FrameworkElementFactory(typeof(Button));
        buttonFactory.SetBinding(ButtonBase.CommandProperty, new Binding(_valuePath));
        buttonFactory.SetValue(FrameworkElement.ToolTipProperty, caption);
        buttonFactory.SetValue(Control.BorderThicknessProperty, new Thickness(0));

        StylingHelpers.SetStyling(buttonFactory, _cellStyling);

        if (_imagePath is not null)
        {
            var imageFactory = new FrameworkElementFactory(typeof(Image));
            imageFactory.SetValue(Image.SourceProperty, Application.Current.FindResource(_imagePath));
            buttonFactory.AppendChild(imageFactory);
        }
        else
        {
            buttonFactory.SetValue(ContentControl.ContentProperty, caption);
        }

        column.CellTemplate = new DataTemplate { VisualTree = buttonFactory };

        return column;
    }
}
