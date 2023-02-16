using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Genius.Atom.UI.Forms.Wpf;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

public sealed class AttachingBehavior : Behavior<DataGrid>
{
    private static readonly IAutoGridColumnBehavior[] _columnBehaviors;
    private readonly Queue<AutoGridColumnContext> _contextsToPostProcess = new();
    private Lazy<AutoGridBuildContext> _autoGridBuildContext = new(() => throw new InvalidOperationException("AutoGridBuilder hasn't been initialized yet."));

    static AttachingBehavior()
    {
        _columnBehaviors = new IAutoGridColumnBehavior[] {
            // Column type changers:
            new ColumnTextBehavior(),
            new ColumnTagEditorBehavior(),
            new ColumnButtonBehavior(),
            new ColumnToggleButtonBehavior(),
            new ColumnWithImageBehavior(),
            new ColumnComboBoxBehavior(),
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
            new ColumnAutoWidthBehavior(),
            new ColumnDisplayIndexBehavior()
        };
    }

    protected override void OnAttached()
    {
        AssociatedObject.AutoGenerateColumns = true;
        AssociatedObject.AutoGeneratingColumn += OnAutoGeneratingColumn;
        AssociatedObject.AutoGeneratedColumns += OnAutoGeneratedColumns;

        AssociatedObject.AddingNewItem += OnAddingNewItem;

        var dpd = DependencyPropertyDescriptor.FromProperty(DataGrid.ItemsSourceProperty, typeof(DataGrid));
        dpd?.AddValueChanged(AssociatedObject, OnItemsSourceChanged);

        var dpd2 = DependencyPropertyDescriptor.FromProperty(Properties.AutoGridBuilderProperty, typeof(DataGrid));
        dpd2?.AddValueChanged(AssociatedObject, OnAutoGridBuilderChanged);

        base.OnAttached();
    }

    private void OnAddingNewItem(object? sender, AddingNewItemEventArgs e)
    {
        e.NewItem = _autoGridBuildContext.Value.RecordFactory.Create();
    }

    private void OnItemsSourceChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject.ItemsSource == null)
        {
            return;
        }

        var rowStyle = CreateRowStyle();

        var listItemType = Helpers.GetListItemType(AssociatedObject.ItemsSource);
        if (AssociatedObject.SelectionMode == DataGridSelectionMode.Extended &&
            typeof(ISelectable).IsAssignableFrom(listItemType))
        {
            BindIsSelected(rowStyle);
        }

        if (typeof(IEditable).IsAssignableFrom(listItemType))
        {
            BindIsEditing(rowStyle);
        }

        AssociatedObject.RowStyle = rowStyle;
    }

    private void OnAutoGridBuilderChanged(object? sender, EventArgs e)
    {
        _autoGridBuildContext = AutoGridBuildContext.CreateLazy(AssociatedObject);

        if (_autoGridBuildContext.Value.EnableVirtualization)
        {
            AssociatedObject.SetValue(VirtualizingPanel.IsVirtualizingProperty, true);
            AssociatedObject.SetValue(VirtualizingPanel.IsVirtualizingWhenGroupingProperty, true);
            AssociatedObject.SetValue(DataGrid.EnableRowVirtualizationProperty, true);
            AssociatedObject.SetValue(ScrollViewer.CanContentScrollProperty, true);
        }

        if (_autoGridBuildContext.Value.MakeReadOnly)
        {
            AssociatedObject.SetValue(DataGrid.IsReadOnlyProperty, true);
        }

        var groupingProperties = _autoGridBuildContext.Value.Columns.Where(x => x.IsGroupedColumn()).ToArray();
        if (groupingProperties.Any())
        {
            if (groupingProperties.Any(x => AutoGridBuilderHelpers.IsGroupableColumn(x.Property)))
            {
                AssociatedObject.GroupStyle.Add((GroupStyle)Application.Current.FindResource("Genius.AutoGrid.Group.GroupableViewModel"));
                AssociatedObject.SetValue(Grid.IsSharedSizeScopeProperty, true);
            }
            else
            {
                AssociatedObject.GroupStyle.Add((GroupStyle)Application.Current.FindResource("Genius.AutoGrid.Group.String"));
            }
        }
    }

    private void OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        var buildColumnContext = _autoGridBuildContext.Value.Columns.FirstOrDefault(x => x.Property.Name.Equals(e.PropertyName));
        if (buildColumnContext is null)
        {
            e.Cancel = true;
            return;
        }

        var context = new AutoGridColumnContext(AssociatedObject, e, buildColumnContext);

        if (AutoGridBuilderHelpers.IsIgnorableProperty(e.PropertyName)
            || buildColumnContext.IsGroupedColumn())
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

        if (context.GetPostProcessingActions().Any())
        {
            _contextsToPostProcess.Enqueue(context);
        }
    }

    private void OnAutoGeneratedColumns(object? sender, EventArgs e)
    {
        while (_contextsToPostProcess.Count > 0)
        {
            var context = _contextsToPostProcess.Dequeue();
            foreach (var postProcessing in context.GetPostProcessingActions())
            {
                postProcessing();
            }
        }
    }

    private void BindIsSelected(Style style)
    {
        var binding = new Binding(nameof(ISelectable.IsSelected));
        style.Setters.Add(new Setter(DataGrid.IsSelectedProperty, binding));
    }

    private void BindIsEditing(Style style)
    {
        var binding = new Binding(nameof(IEditable.IsEditing));
        style.Setters.Add(new Setter(Properties.IsEditingProperty, binding));

        AssociatedObject.BeginningEdit += (sender, e) =>
        {
            if (e.Row.Item is IEditable editable)
            {
                e.Row.SetValue(Properties.IsEditingHandlingSuspendedProperty, true);
                editable.IsEditing = true;
                e.Row.SetValue(Properties.IsEditingHandlingSuspendedProperty, false);
            }
        };
        AssociatedObject.RowEditEnding += (sender, e) =>
        {
            if (e.Row.Item is IEditable editable)
            {
                e.Row.SetValue(Properties.IsEditingHandlingSuspendedProperty, true);
                editable.IsEditing = false;
                e.Row.SetValue(Properties.IsEditingHandlingSuspendedProperty, false);
            }
        };
    }

    private Style CreateRowStyle()
    {
        var rowStyle = new Style {
            TargetType = typeof(DataGridRow),
            BasedOn = (Style) AssociatedObject.FindResource("MahApps.Styles.DataGridRow")
        };

        StylingHelpers.CopyStyle(AssociatedObject.RowStyle, rowStyle);

        return rowStyle;
    }
}
