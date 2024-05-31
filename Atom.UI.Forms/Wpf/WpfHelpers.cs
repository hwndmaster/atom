using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Wpf;

[ExcludeFromCodeCoverage]
public static class WpfHelpers
{
    /// <summary>
    ///   Adds a flyout popup to the window, which <paramref name="owner"/> relates to.
    /// </summary>
    /// <typeparam name="T">The type of the flyout control to popup.</typeparam>
    /// <param name="owner">The owner control, to which data context it binds and which window will be used as a container.</param>
    /// <param name="isOpenBindingPath">The name of the property, which represents a boolean value indicating whether the flyout is visible or not.</param>
    /// <param name="sourcePath">The name of the property, which is used as a data context for the flyout.</param>
    public static void AddFlyout<T>(FrameworkElement owner, string isOpenBindingPath, string? sourcePath = null)
        where T: Flyout, new()
    {
        Guard.NotNull(owner);

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

    /// <summary>
    ///   Checks if the specified <paramref name="dataGrid"/> has a context menu defined, and
    ///   adds one if it was not.
    /// </summary>
    /// <param name="dataGrid">The data grid.</param>
    /// <returns>The context menu, whether determined in the <paramref name="dataGrid"/> or created in place.</returns>
    public static ContextMenu EnsureDataGridRowContextMenu(DataGrid dataGrid)
    {
        Guard.NotNull(dataGrid);

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

    /// <summary>
    ///   Returns a size of the candidate text, measured for the specified <paramref name="refElement"/>.
    /// </summary>
    /// <param name="candidate">The candidate text.</param>
    /// <param name="refElement">The reference control.</param>
    /// <returns>The size of the candidate text.</returns>
    public static Size MeasureString(string candidate, Control refElement)
    {
        Guard.NotNull(refElement);

        var formattedText = new FormattedText(
            candidate,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(refElement.FontFamily, refElement.FontStyle, refElement.FontWeight, refElement.FontStretch),
            refElement.FontSize,
            refElement.Foreground,
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
                var dispatcher = Module.ServiceProvider.GetRequiredService<IUiDispatcher>();
                dispatcher.Invoke(() => comboBox.IsDropDownOpen = true);
            }
        }
    }
}
