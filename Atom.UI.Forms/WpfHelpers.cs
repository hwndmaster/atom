using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Genius.Atom.UI.Forms.Controls.TagEditor;
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
            ((IAddChild) flyout).AddChild(child);
        }

        internal static DataGridTemplateColumn CreateButtonColumn(string commandPath, string iconName)
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

            return new DataGridTemplateColumn
            {
                CellTemplate = new DataTemplate { VisualTree = buttonFactory }
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
            if (column.CellStyle == null)
            {
                column.CellStyle = new Style {
                    TargetType = typeof(DataGridCell),
                    BasedOn = (Style) Application.Current.FindResource("MahApps.Styles.DataGridCell")
                };
            }

            return column.CellStyle;
        }

        internal static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
                yield break;

            if (dependencyObject is T t)
                yield return t;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
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
