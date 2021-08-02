using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace Genius.Atom.UI.Forms
{
    [ExcludeFromCodeCoverage]
    public static class WpfHelpers
    {
        public static void AddFlyout<T>(FrameworkElement owner, string isOpenBindingPath, string sourcePath = null)
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
            (flyout as IAddChild).AddChild(child);
        }

        public static DataGridTemplateColumn CreateButtonColumn(string commandPath, string iconName)
        {
            var caption = Helpers.MakeCaptionFromPropertyName(commandPath.Replace("Command", ""));

            var buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetBinding(Button.CommandProperty, new Binding(commandPath));
            buttonFactory.SetValue(Button.ToolTipProperty, caption);
            buttonFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
            if (iconName != null)
            {
                var imageFactory = new FrameworkElementFactory(typeof(Image));
                imageFactory.SetValue(Image.SourceProperty, Application.Current.FindResource(iconName));
                buttonFactory.AppendChild(imageFactory);
            }
            else
            {
                buttonFactory.SetValue(Button.ContentProperty, caption);
            }

            var column = new DataGridTemplateColumn();
            column.CellTemplate = new DataTemplate { VisualTree = buttonFactory };
            return column;
        }

        public static DataGridComboBoxColumn CreateComboboxColumnWithStaticItemsSource(IEnumerable itemsSource, string valuePath)
        {
            var column = new DataGridComboBoxColumn();
            column.Header = valuePath;
            column.ItemsSource = itemsSource;
            column.SelectedValueBinding = new Binding(valuePath);
            return column;
        }

        public static void EnableSingleClickEditMode(DataGridColumn column)
        {
            MouseButtonEventHandler del1 = (object sender, MouseButtonEventArgs e) =>
                {
                    var cell = sender as DataGridCell;
                    GridColumnFastEdit(cell, e);
                };
            TextCompositionEventHandler del2 = (object sender, TextCompositionEventArgs e) =>
                {
                    var cell = sender as DataGridCell;
                    GridColumnFastEdit(cell, e);
                };

            EnsureDefaultCellStyle(column);
            column.CellStyle.Setters.Add(new EventSetter(UIElement.PreviewMouseLeftButtonDownEvent, del1));
            column.CellStyle.Setters.Add(new EventSetter(UIElement.PreviewTextInputEvent, del2));
        }

        public static Style EnsureDefaultCellStyle(DataGridColumn column)
        {
            if (column.CellStyle == null)
            {
                column.CellStyle = new Style {
                    TargetType = typeof(DataGridCell),
                    BasedOn = (Style) Application.Current.FindResource("MahApps.Styles.DataGridCell")
                };
            }

            return column.CellStyle;
        }

        public static void SetCellHorizontalAlignment(DataGridColumn column, HorizontalAlignment alignment)
        {
            EnsureDefaultCellStyle(column);

            column.CellStyle.Setters.Add(new Setter(DataGridCell.HorizontalAlignmentProperty, alignment));
        }

        public static Color ChangeColorBrightness(Color color, float correctionFactor)
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

        private static void GridColumnFastEdit(DataGridCell cell, RoutedEventArgs e)
        {
            if (cell == null || cell.IsEditing || cell.IsReadOnly)
                return;

            DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
            if (dataGrid == null)
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
                    DataGridRow row = FindVisualParent<DataGridRow>(cell);
                    if (row != null && !row.IsSelected)
                    {
                        row.IsSelected = true;
                    }
                }
            }
            else
            {
                dataGrid.BeginEdit(e);

                var comboBox = FindFirstChild<ComboBox>(cell);
                if (comboBox != null)
                {
                    cell.Dispatcher.Invoke(
                        new Action(delegate {
                            comboBox.IsDropDownOpen = true;
                        }));
                }
            }
        }

        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                if (parent is T correctlyTyped)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        private static T FindFirstChild<T>(FrameworkElement element) where T: FrameworkElement
        {
            if (element == null)
                return null;
            var count = VisualTreeHelper.GetChildrenCount(element);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                if (child is T childAsT)
                    return childAsT;

                var foundInner = FindFirstChild<T>(child);
                if (foundInner != null)
                    return foundInner;
            }

            return null;
        }
    }
}
