using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

public class AttachingBehavior : Behavior<DataGrid>
{
    private bool? _showOnlyBrowsable;

    private static readonly IAutoGridColumnBehavior[] _columnBehaviors;

    static AttachingBehavior()
    {
        _columnBehaviors = new IAutoGridColumnBehavior[] {
            // Column type changers:
            new ColumnTagEditorBehavior(),
            new ColumnButtonBehavior(),
            new ColumnWithImageBehavior(),
            new ColumnComboboxBehavior(),
            new ColumnAttachedViewBehavior(),

            // Binding changers:
            new ColumnConverterBehavior(),
            new ColumnFormattingBehavior(),
            new ColumnNullableBehavior(),

            // Style changers:
            new ColumnTooltipBehavior(),
            new ColumnStylingBehavior(),
            new ColumnValidationBehavior(),

            // Misc:
            new ColumnHeaderNameBehavior(),
            new ColumnReadOnlyBehavior(),
            new ColumnDisplayIndexBehavior()
        };
    }

    protected override void OnAttached()
    {
        AssociatedObject.AutoGenerateColumns = true;
        AssociatedObject.AutoGeneratingColumn += OnAutoGeneratingColumn;

        var dpd = DependencyPropertyDescriptor.FromProperty(DataGrid.ItemsSourceProperty, typeof(DataGrid));
        dpd?.AddValueChanged(AssociatedObject, OnItemsSourceChanged);

        base.OnAttached();
    }

    private void OnItemsSourceChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject.ItemsSource == null)
        {
            return;
        }

        var listItemType = Helpers.GetListItemType(AssociatedObject.ItemsSource);
        if (AssociatedObject.SelectionMode == DataGridSelectionMode.Extended &&
            typeof(ISelectable).IsAssignableFrom(listItemType))
        {
            BindIsSelected();
        }
    }

    private void OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        var context = new AutoGridColumnContext(AssociatedObject, e, (PropertyDescriptor) e.PropertyDescriptor);

        if (!IsBrowsable(context))
        {
            e.Cancel = true;
            return;
        }

        var ignoreProperties = new [] {
            nameof(IHasDirtyFlag.IsDirty),
            nameof(ISelectable.IsSelected),
            nameof(INotifyDataErrorInfo.HasErrors)
        };

        if (ignoreProperties.Contains(e.PropertyName)
            || context.GetAttribute<GroupByAttribute>() != null)
            //|| typeof(ICollection).IsAssignableFrom(context.Property.PropertyType))
        {
            e.Cancel = true;
            return;
        }

        foreach (var columnBehavior in _columnBehaviors)
        {
            columnBehavior.Attach(context);
        }

        WpfHelpers.EnableSingleClickEditMode(e.Column);
    }

    private bool IsBrowsable(AutoGridColumnContext context)
    {
        _showOnlyBrowsable ??= context.Property.ComponentType.GetCustomAttributes(false)
            .Any(x => x is ShowOnlyBrowsableAttribute b && b.OnlyBrowsable);

        var browsable = context.GetAttribute<BrowsableAttribute>();
        return (!_showOnlyBrowsable.Value || browsable?.Browsable == true)
            && (_showOnlyBrowsable.Value || browsable?.Browsable != false);
    }

    private void BindIsSelected()
    {
        var binding = new Binding(nameof(ISelectable.IsSelected));
        var rowStyle = new Style {
            TargetType = typeof(DataGridRow),
            BasedOn = (Style) AssociatedObject.FindResource("MahApps.Styles.DataGridRow")
        };
        rowStyle.Setters.Add(new Setter(DataGrid.IsSelectedProperty, binding));
        AssociatedObject.RowStyle = rowStyle;
    }
}
