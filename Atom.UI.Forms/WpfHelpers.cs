using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Genius.Atom.UI.Forms.Behaviors;
using Genius.Atom.UI.Forms.Controls.TagEditor;
using Genius.Atom.UI.Forms.WpfBuilders;
using MahApps.Metro.Controls;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms;

[ExcludeFromCodeCoverage]
public static class WpfHelpers
{
    public static void AddFlyout<T>(FrameworkElement owner, string isOpenBindingPath, string? sourcePath = null)
        where T: Flyout, new()
    {
        var parentWindow = Window.GetWindow(owner);
        object obj = parentWindow.FindName("flyoutsControl");
        var flyout = (FlyoutsControl) obj;
        var child = new T();
        if (sourcePath == null)
        {
            child.DataContext = owner.DataContext;
        }
        else
        {
            BindingOperations.SetBinding(child, Flyout.DataContextProperty,
                new Binding(sourcePath) { Source = owner.DataContext });
            //: TypeDescriptor.GetProperties(owner.DataContext).Find(sourcePath, false).GetValue(owner.DataContext);
        }
        BindingOperations.SetBinding(child, Flyout.IsOpenProperty, new Binding(isOpenBindingPath) { Source = owner.DataContext });
        ((IAddChild) flyout).AddChild(child);
    }

    [Obsolete("Use `WpfExtensions.FindVisualChildren<T>` extension method instead")]
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
    {
        return dependencyObject.FindVisualChildren<T>();
    }

    [Obsolete("Use `WpfExtensions.FindVisualParent<T>` extension method instead")]
    public static T? FindVisualParent<T>(UIElement element) where T : UIElement
    {
        return element.FindVisualParent<T>();
    }

    internal static DataGridTemplateColumn CreateButtonColumn(string commandPath, StylingRecord? styling, string? iconName)
    {
        var caption = Helpers.MakeCaptionFromPropertyName(commandPath.Replace("Command", ""));

        var buttonFactory = new FrameworkElementFactory(typeof(Button));
        buttonFactory.SetBinding(Button.CommandProperty, new Binding(commandPath));
        buttonFactory.SetValue(Button.ToolTipProperty, caption);
        buttonFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0));

        SetStyling(buttonFactory, styling);

        if (iconName is not null)
        {
            var imageFactory = new FrameworkElementFactory(typeof(Image));
            imageFactory.SetValue(Image.SourceProperty, Application.Current.FindResource(iconName));
            buttonFactory.AppendChild(imageFactory);
        }
        else
        {
            buttonFactory.SetValue(Button.ContentProperty, caption);
        }

        return new DataGridTemplateColumn
        {
            CellTemplate = new DataTemplate { VisualTree = buttonFactory }
        };
    }

    internal static DataGridTemplateColumn CreateToggleSwitchColumn(string propertyPath, string? iconForTrue, string? iconForFalse, StylingRecord? styling = null)
    {
        FrameworkElementFactory elementFactory;

        if (iconForTrue is not null && iconForFalse is not null)
        {
            elementFactory = new FrameworkElementFactory(typeof(ToggleButton));
            elementFactory.SetBinding(ToggleButton.IsCheckedProperty, new Binding(propertyPath)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            elementFactory.SetValue(ToggleButton.WidthProperty, 22d);
            elementFactory.SetValue(ToggleButton.HeightProperty, 22d);
            elementFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0));

            var iconForTrueResource = Application.Current.FindResource(iconForTrue);
            var iconForFalseResource = Application.Current.FindResource(iconForFalse);

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
                BindingOperations.SetBinding(behavior, ImageConditionalSourceBehavior.FlagValueProperty, new Binding(propertyPath));
                Interaction.GetBehaviors(imageElement).Add(behavior);
            }
        }
        else
        {
            elementFactory = new FrameworkElementFactory(typeof(ToggleSwitch));
            elementFactory.SetBinding(ToggleSwitch.IsOnProperty, new Binding(propertyPath)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            elementFactory.SetValue(ToggleSwitch.OnContentProperty, string.Empty);
            elementFactory.SetValue(ToggleSwitch.OffContentProperty, string.Empty);
            elementFactory.SetValue(FrameworkElement.MinWidthProperty, 22d);
        }

        var caption = Helpers.MakeCaptionFromPropertyName(propertyPath);
        elementFactory.SetValue(FrameworkElement.ToolTipProperty, caption);

        SetStyling(elementFactory, styling);

        return new DataGridTemplateColumn
        {
            CellTemplate = new DataTemplate { VisualTree = elementFactory }
        };
    }

    internal static DataGridTemplateColumn CreateTagEditorColumn(string headerName, string valuePath)
    {
        var column = new DataGridTemplateColumn {
            Header = headerName
        };

        var bindToValue = new Binding(valuePath);

        column.CellTemplate = CreateTagEditorDataTemplate(bindToValue, @readonly: true);
        column.CellEditingTemplate = CreateTagEditorDataTemplate(bindToValue, @readonly: false);

        return column;
    }

    internal static DataGridComboBoxColumn CreateComboboxColumnWithStaticItemsSource(IEnumerable itemsSource, string valuePath)
    {
        return new DataGridComboBoxColumn
        {
            Header = valuePath,
            ItemsSource = itemsSource,
            SelectedValueBinding = new Binding(valuePath)
        };
    }

    // TODO: Not used yet
    /*internal static void AutoFitColumn(DataGridColumn column, IEnumerable items)
    {
        var childControl = column.FindChild<Control>();
        var maxWidth = column.MinWidth;
        foreach (var item in items)
        {
            if (item is null)
                continue;
            maxWidth = Math.Max(maxWidth, MeasureString(item.ToString().NotNull(), childControl).Width);
        }

        column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
    }*/

    private static DataTemplate CreateTagEditorDataTemplate(Binding bindToValue, bool @readonly)
    {
        var tagEditorFactory = new FrameworkElementFactory(typeof(TagEditor));
        tagEditorFactory.SetBinding(FrameworkElement.DataContextProperty, bindToValue);
        if (@readonly)
        {
            tagEditorFactory.SetValue(ControlsHelper.IsReadOnlyProperty, true);
        }
        return new DataTemplate { VisualTree = tagEditorFactory };
    }

    internal static void EnableSingleClickEditMode(DataGridColumn column)
    {
        MouseButtonEventHandler del1 = (object sender, MouseButtonEventArgs e) => {
            var cell = sender as DataGridCell;
            GridColumnFastEdit(cell, e);
        };
        TextCompositionEventHandler del2 = (object sender, TextCompositionEventArgs e) => {
            var cell = sender as DataGridCell;
            GridColumnFastEdit(cell, e);
        };

        EnsureDefaultCellStyle(column);
        column.CellStyle.Setters.Add(new EventSetter(UIElement.PreviewMouseLeftButtonDownEvent, del1));
        column.CellStyle.Setters.Add(new EventSetter(UIElement.PreviewTextInputEvent, del2));
    }

    internal static Style EnsureDefaultCellStyle(DataGridColumn column)
    {
        if (column.CellStyle is null)
        {
            column.CellStyle = new Style {
                TargetType = typeof(DataGridCell),
                BasedOn = (Style) Application.Current.FindResource("MahApps.Styles.DataGridCell")
            };
        }

        return column.CellStyle;
    }

    internal static void SetCellHorizontalAlignment(DataGridColumn column, HorizontalAlignment alignment)
    {
        EnsureDefaultCellStyle(column);

        column.CellStyle.Setters.Add(new Setter(DataGridCell.HorizontalAlignmentProperty, alignment));
    }

    internal static Color ChangeColorBrightness(Color color, float correctionFactor)
    {
        float red = (float)color.R;
        float green = (float)color.G;
        float blue = (float)color.B;

        if (correctionFactor < 0)
        {
            correctionFactor = 1 + correctionFactor;
            red *= correctionFactor;
            green *= correctionFactor;
            blue *= correctionFactor;
        }
        else
        {
            red = (255 - red) * correctionFactor + red;
            green = (255 - green) * correctionFactor + green;
            blue = (255 - blue) * correctionFactor + blue;
        }

        return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
    }

    // TODO: Not used yet
    internal static Size MeasureString(string candidate, Control refElement)
    {
        var formattedText = new FormattedText(
            candidate,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(refElement.FontFamily, refElement.FontStyle, refElement.FontWeight, refElement.FontStretch),
            refElement.FontSize,
            Brushes.Black,
            new NumberSubstitution(),
            1);

        return new Size(formattedText.Width, formattedText.Height);
    }

    private static void GridColumnFastEdit(DataGridCell? cell, RoutedEventArgs e)
    {
        if (cell is null || cell.IsEditing || cell.IsReadOnly)
            return;

        DataGrid? dataGrid = cell.FindVisualParent<DataGrid>();
        if (dataGrid is null)
            return;

        if (!cell.IsFocused)
        {
            cell.Focus();
        }

        if (cell.Content is CheckBox)
        {
            if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
            {
                if (!cell.IsSelected)
                    cell.IsSelected = true;
            }
            else
            {
                DataGridRow? row = cell.FindVisualParent<DataGridRow>();
                if (row is not null && !row.IsSelected)
                {
                    row.IsSelected = true;
                }
            }
        }
        else
        {
            dataGrid.BeginEdit(e);

            var comboBox = FindFirstChild<ComboBox>(cell);
            if (comboBox is not null)
            {
                cell.Dispatcher.Invoke(
                    new Action(delegate {
                        comboBox.IsDropDownOpen = true;
                    }));
            }
        }
    }

    private static T? FindFirstChild<T>(FrameworkElement? element) where T: FrameworkElement
    {
        if (element is null)
            return null;
        var count = VisualTreeHelper.GetChildrenCount(element);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
            if (child is T childAsT)
                return childAsT;

            var foundInner = FindFirstChild<T>(child);
            if (foundInner is not null)
                return foundInner;
        }

        return null;
    }

    private static void SetStyling(FrameworkElementFactory elementFactory, StylingRecord? styling)
    {
        if (styling is null) return;

        if (styling.HorizontalAlignment is not null)
            elementFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, styling.HorizontalAlignment);
        if (styling.Margin is not null)
            elementFactory.SetValue(FrameworkElement.MarginProperty, styling.Margin);
        if (styling.Padding is not null)
            elementFactory.SetValue(Control.PaddingProperty, styling.Padding);
    }
}
