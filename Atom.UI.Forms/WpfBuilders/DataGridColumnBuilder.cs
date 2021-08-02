using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Genius.Atom.UI.Forms.ValueConverters;
using WpfAnimatedGif;

namespace Genius.Atom.UI.Forms.WpfBuilders
{
    public class DataGridColumnBuilder
    {
        private readonly string _valuePath;
        private IValueConverter _converter;
        private string _itemsSourcePath;
        private string _imageSource;
        private double _imageSize;
        private bool _hideText;

        private DataGridColumnBuilder(string valuePath)
        {
            _valuePath = valuePath;
        }

        public static DataGridColumnBuilder ForValuePath(string valuePath)
        {
            return new DataGridColumnBuilder(valuePath);
        }

        public DataGridColumnBuilder WithConverter(IValueConverter converter)
        {
            _converter = converter;
            return this;
        }

        public DataGridColumnBuilder WithComboEditor(string itemsSourcePath)
        {
            _itemsSourcePath = itemsSourcePath;
            return this;
        }

        public DataGridColumnBuilder WithImageSource(string imageSource, double? imageSize = null, bool hideText = false)
        {
            if (imageSource == null)
                return this;
            _imageSource = imageSource;
            _imageSize = imageSize ?? 16;
            _hideText = hideText;
            return this;
        }

        public DataGridTemplateColumn Build()
        {
            var column = new DataGridTemplateColumn {
                Header = _valuePath,
                SortMemberPath = _valuePath
            };

            var bindToValue = new Binding(_valuePath);
            if (_converter != null || _itemsSourcePath != null)
                bindToValue.Converter = _converter ?? new PropertyValueStringConverter();

            column.CellTemplate = _imageSource == null
                ? CreateTextTemplate(bindToValue)
                : CreateTextWithImageTemplate(bindToValue, hideText: _hideText);

            if (_itemsSourcePath != null)
                column.CellEditingTemplate = CreateComboEditor(bindToValue);

            return column;
        }

        private DataTemplate CreateTextTemplate(Binding bindToValue)
        {
            var textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetBinding(TextBlock.TextProperty, bindToValue);
            return new DataTemplate { VisualTree = textFactory };
        }

        private DataTemplate CreateTextWithImageTemplate(Binding bindToValue, string imageTooltip = null, bool imageIsPath = true,
            string imageVisibilityFlagPath = null, bool hideText = false)
        {
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var imageFactory = new FrameworkElementFactory(typeof(Image));

            if (imageIsPath)
                imageFactory.SetBinding(ImageBehavior.AnimatedSourceProperty, new Binding(_imageSource));
            else
                imageFactory.SetValue(ImageBehavior.AnimatedSourceProperty, (BitmapImage)Application.Current.FindResource(_imageSource));

            if (imageTooltip != null)
                imageFactory.SetValue(Image.ToolTipProperty, imageTooltip);

            if (imageVisibilityFlagPath != null)
                imageFactory.SetValue(Image.VisibilityProperty, new Binding(imageVisibilityFlagPath) { Converter = new BooleanToVisibilityConverter() });

            if (_imageSize != 0)
            {
                imageFactory.SetValue(Image.HeightProperty, _imageSize);
                imageFactory.SetValue(Image.WidthProperty, _imageSize);
            }

            stackPanelFactory.AppendChild(imageFactory);

            if (!hideText)
            {
                var textFactory = new FrameworkElementFactory(typeof(TextBlock));
                textFactory.SetBinding(TextBlock.TextProperty, bindToValue);
                textFactory.SetValue(Image.MarginProperty, new Thickness(5, 0, 0, 0));
                stackPanelFactory.AppendChild(textFactory);
            }

            return new DataTemplate { VisualTree = stackPanelFactory };
        }

        private DataTemplate CreateComboEditor(Binding bindToValue)
        {
            var bindToItemsSource = new Binding(_itemsSourcePath);

            var comboFactory = new FrameworkElementFactory(typeof(ComboBox));
            comboFactory.SetValue(ComboBox.IsTextSearchEnabledProperty, true);
            comboFactory.SetBinding(ComboBox.SelectedItemProperty, bindToValue);
            comboFactory.SetBinding(ComboBox.ItemsSourceProperty, bindToItemsSource);

            return new DataTemplate { VisualTree = comboFactory };
        }
    }
}