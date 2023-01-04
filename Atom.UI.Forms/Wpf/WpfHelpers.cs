using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace Genius.Atom.UI.Forms.Wpf;

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

        StylingHelpers.EnsureDefaultCellStyle(column);
        column.CellStyle.Setters.Add(new EventSetter(UIElement.PreviewMouseLeftButtonDownEvent, del1));
        column.CellStyle.Setters.Add(new EventSetter(UIElement.PreviewTextInputEvent, del2));
    }

    public static ContextMenu EnsureDataGridRowContextMenu(DataGrid dataGrid)
    {
        if (dataGrid.RowStyle?.Setters
            .OfType<Setter>()
            .FirstOrDefault(x => x.Property == FrameworkElement.ContextMenuProperty)
            ?.Value is not ContextMenu contextMenu)
        {
            contextMenu = new ContextMenu();
            var rowStyle = new Style(typeof(DataGridRow), dataGrid.RowStyle);
            rowStyle.Setters.Add(new Setter(FrameworkElement.ContextMenuProperty, contextMenu));
            dataGrid.RowStyle = rowStyle;
        }

        return contextMenu;
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

            var comboBox = cell.FindChild<ComboBox>();
            if (comboBox is not null)
            {
                cell.Dispatcher.Invoke(
                    new Action(delegate {
                        comboBox.IsDropDownOpen = true;
                    }));
            }
        }
    }
}
