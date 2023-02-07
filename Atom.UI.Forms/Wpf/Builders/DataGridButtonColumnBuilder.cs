using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Wpf.Builders;

internal sealed class DataGridButtonColumnBuilder : DataGridColumnBuilder
{
    private readonly string? _imageResourceName;
    private readonly Size? _imageSize;

    internal DataGridButtonColumnBuilder(DataGridColumnBuilder parentBuilder, string? imageResourceName, Size? imageSize = null)
        : base(parentBuilder.NotNull())
    {
        _imageResourceName = imageResourceName;
        _imageSize = imageSize;
    }

    public override DataGridTemplateColumn Build()
    {
        var column = CreateColumn();
        var caption = Helpers.MakeCaptionFromPropertyName(_valuePath.Replace("Command", ""));

        var buttonFactory = new FrameworkElementFactory(typeof(Button));
        buttonFactory.SetBinding(ButtonBase.CommandProperty, new Binding(_valuePath));

        SetupToolTip(buttonFactory, caption);
        buttonFactory.SetValue(Control.BorderThicknessProperty, new Thickness(0));

        StylingHelpers.SetStyling(buttonFactory, _cellStyling);

        if (_imageResourceName is not null)
        {
            var imageFactory = new FrameworkElementFactory(typeof(Image));
            imageFactory.SetValue(Image.SourceProperty, Application.Current.FindResource(_imageResourceName));
            if (_imageSize is not null)
            {
                imageFactory.SetValue(Image.WidthProperty, _imageSize.Value.Width);
                imageFactory.SetValue(Image.HeightProperty, _imageSize.Value.Height);
            }
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
