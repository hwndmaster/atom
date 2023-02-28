using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms.Wpf.Builders;

internal sealed class DataGridToggleImageButtonColumnBuilder : DataGridColumnBuilder
{
    private readonly string _imageForTrue;
    private readonly string _imageForFalse;

    internal DataGridToggleImageButtonColumnBuilder(DataGridColumnBuilder parentBuilder, string imageForTrue, string imageForFalse)
        : base(parentBuilder.NotNull())
    {
        _imageForTrue = imageForTrue.NotNull();
        _imageForFalse = imageForFalse.NotNull();
    }

    public override DataGridTemplateColumn Build()
    {
        var column = CreateColumn();
        var binding = CreateBinding();
        binding.Mode = BindingMode.TwoWay;
        binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

        var elementFactory = new FrameworkElementFactory(typeof(ToggleButton));
        elementFactory.SetBinding(ToggleButton.IsCheckedProperty, binding);
        elementFactory.SetValue(FrameworkElement.WidthProperty, 22d);
        elementFactory.SetValue(FrameworkElement.HeightProperty, 22d);
        elementFactory.SetValue(Control.BorderThicknessProperty, new Thickness(0));

        var iconForTrueResource = Application.Current.FindResource(_imageForTrue);
        var iconForFalseResource = Application.Current.FindResource(_imageForFalse);

        var imageFactory = new FrameworkElementFactory(typeof(Image));
        imageFactory.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnImageLoaded));
        elementFactory.AppendChild(imageFactory);

        void OnImageLoaded(object sender, RoutedEventArgs args)
        {
            var imageElement = (Image)sender;
            var behavior = new ImageConditionalSourceBehavior
            {
                WhenTrue = iconForTrueResource,
                WhenFalse = iconForFalseResource
            };
            BindingOperations.SetBinding(behavior, ImageConditionalSourceBehavior.FlagValueProperty, new Binding(_valuePath));
            Interaction.GetBehaviors(imageElement).Add(behavior);
        }

        StylingHelpers.SetStyling(elementFactory, _cellStyling);

        var caption = Helpers.MakeCaptionFromPropertyName(_valuePath);
        elementFactory.SetValue(FrameworkElement.ToolTipProperty, caption);

        column.CellTemplate = new DataTemplate { VisualTree = elementFactory };

        return column;
    }
}
