using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using WpfAnimatedGif;

namespace Genius.Atom.UI.Forms.Wpf.Builders;

internal sealed class DataGridTextWithImageColumnBuilder : DataGridTextColumnBuilder
{
    private readonly string? _imageSource;
    private double _imageSize = 16;
    private bool _hideText = false;
    private bool _imageIsPath = true;
    private string? _imageTooltip = null;
    private string? _imageVisibilityFlagPath = null;

    internal DataGridTextWithImageColumnBuilder(DataGridColumnBuilder parentBuilder, string imageSource)
        : base(parentBuilder.NotNull())
    {
        _imageSource = imageSource.NotNull();
    }

    public DataGridTextWithImageColumnBuilder TreatImageAsResourceName()
    {
        _imageIsPath = false;
        return this;
    }

    public DataGridTextWithImageColumnBuilder WithImageTooltip(string tooltip)
    {
        _imageTooltip = tooltip;
        return this;
    }

    public DataGridTextWithImageColumnBuilder WithImageSize(double? imageSize)
    {
        if (imageSize is not null)
        {
            _imageSize = imageSize.Value;
        }

        return this;
    }

    public DataGridTextWithImageColumnBuilder WithImageVisibilityFlagPath(string imageVisibilityFlagPath)
    {
        _imageVisibilityFlagPath = imageVisibilityFlagPath;
        return this;
    }

    public DataGridTextWithImageColumnBuilder WithTextHidden(bool hideText)
    {
        _hideText = hideText;
        return this;
    }

    public override DataGridTemplateColumn Build()
    {
        var column = CreateColumn();

        var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
        stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

        var imageFactory = new FrameworkElementFactory(typeof(Image));

        if (_imageIsPath)
        {
            imageFactory.SetBinding(ImageBehavior.AnimatedSourceProperty, new Binding(_imageSource)
            {
                Converter = new ImageSourceConverter()
            });
            imageFactory.SetBinding(UIElement.VisibilityProperty, new Binding(_imageSource)
            {
                Converter = new NotNullToVisibilityConverter()
            });
        }
        else
        {
            imageFactory.SetValue(ImageBehavior.AnimatedSourceProperty, (BitmapImage)Application.Current.FindResource(_imageSource));
        }

        if (_imageTooltip is not null)
        {
            imageFactory.SetValue(FrameworkElement.ToolTipProperty, _imageTooltip);
        }

        if (_imageVisibilityFlagPath is not null)
        {
            imageFactory.SetValue(UIElement.VisibilityProperty, new Binding(_imageVisibilityFlagPath)
            {
                Converter = new BooleanToVisibilityConverter()
            });
        }

        if (!_imageSize.IsZero())
        {
            imageFactory.SetValue(FrameworkElement.HeightProperty, _imageSize);
            imageFactory.SetValue(FrameworkElement.WidthProperty, _imageSize);
        }

        stackPanelFactory.AppendChild(imageFactory);

        if (!_hideText)
        {
            var textBlock = RenderTextBlock();
            textBlock.SetValue(FrameworkElement.MarginProperty, new Thickness(5, 0, 0, 0));
            stackPanelFactory.AppendChild(textBlock);
        }

        column.CellTemplate = new DataTemplate { VisualTree = stackPanelFactory };

        return column;
    }
}
