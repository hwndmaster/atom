using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Controls.AutoGrid;

namespace Genius.Atom.UI.Forms.Wpf.Builders;

internal class DataGridColumnBuilder
{
    protected readonly string _valuePath;
    protected StylingRecord? _cellStyling;
    protected bool _readOnly;
    private string? _title;
    private string? _toolTip;
    private string? _toolTipPath;
    private IValueConverter? _converter;
    private string? _itemsSourcePath;

    protected DataGridColumnBuilder(DataGridColumnBuilder parent)
    {
        _valuePath = parent._valuePath;
        _readOnly = parent._readOnly;
        _title = parent._title;
        _toolTip = parent._toolTip;
        _toolTipPath = parent._toolTipPath;
        _converter = parent._converter;
        _itemsSourcePath = parent._itemsSourcePath;
        _cellStyling = parent._cellStyling;
    }

    private DataGridColumnBuilder(string valuePath)
    {
        _valuePath = valuePath.NotNull(nameof(valuePath));
    }

    public static DataGridColumnBuilder ForValuePath(string valuePath)
    {
        return new DataGridColumnBuilder(valuePath);
    }

    public DataGridColumnBuilder AsReadOnly(bool readOnly)
    {
        _readOnly = readOnly;
        return this;
    }

    public DataGridColumnBuilder BasedOnAutoGridColumnContext(AutoGridColumnContext context)
    {
        return AsReadOnly(context.IsReadOnly || context.DataGrid.IsReadOnly)
            .WithConverter(context.BuildColumn.ValueConverter)
            .WithCellStyling(context.BuildColumn.Style)
            .WithTitle(context.BuildColumn.DisplayName)
            .WithToolTip(context.BuildColumn.ToolTip)
            .WithToolTipPath(context.BuildColumn.ToolTipPath);
    }

    public DataGridColumnBuilder WithConverter(IValueConverter? converter)
    {
        _converter = converter;
        return this;
    }

    public DataGridColumnBuilder WithComboEditor(string itemsSourcePath)
    {
        _itemsSourcePath = itemsSourcePath;
        return this;
    }

    public DataGridColumnBuilder WithCellStyling(StylingRecord? styling)
    {
        _cellStyling = styling;
        return this;
    }

    public DataGridColumnBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public DataGridColumnBuilder WithToolTip(string? toolTip)
    {
        _toolTip = toolTip;
        return this;
    }

    public DataGridColumnBuilder WithToolTipPath(string? toolTipPath)
    {
        _toolTipPath = toolTipPath;
        return this;
    }

    public DataGridButtonColumnBuilder RenderAsButton(string? imagePath, Size? imageSize = null)
    {
        return new DataGridButtonColumnBuilder(this, imagePath, imageSize);
    }

    public DataGridTagEditorColumnBuilder RenderAsTagEditor()
    {
        return new DataGridTagEditorColumnBuilder(this);
    }

    public DataGridTextColumnBuilder RenderAsText()
    {
        return new DataGridTextColumnBuilder(this);
    }

    public DataGridTextWithImageColumnBuilder RenderAsTextWithImage(string imageSource)
    {
        return new DataGridTextWithImageColumnBuilder(this, imageSource);
    }

    public DataGridToggleImageButtonColumnBuilder RenderAsToggleImageButton(string imageForTrue, string imageForFalse)
    {
        return new DataGridToggleImageButtonColumnBuilder(this, imageForTrue, imageForFalse);
    }

    public DataGridToggleSwitchColumnBuilder RenderAsToggleSwitch()
    {
        return new DataGridToggleSwitchColumnBuilder(this);
    }

    public DataGridViewContentColumnBuilder RenderAsViewContent(Type viewType)
    {
        return new DataGridViewContentColumnBuilder(this, viewType);
    }

    public virtual DataGridTemplateColumn Build()
    {
        var column = CreateColumn();
        var binding = CreateBinding();

        column.CellTemplate = CreateTextTemplate(binding);

        if (_itemsSourcePath is not null)
        {
            column.CellEditingTemplate = CreateComboEditor(binding);
        }

        return column;
    }

    protected Binding CreateBinding()
    {
        var binding = new Binding(_valuePath)
        {
            Converter = _converter ?? new PropertyValueStringConverter(null)
        };

        if (binding.Converter is null && _itemsSourcePath is not null)
        {
            binding.Converter = new PropertyValueStringConverter(null);
        }

        return binding;
    }

    protected DataGridTemplateColumn CreateColumn()
    {
        return new DataGridTemplateColumn {
            Header = _title ?? Helpers.MakeCaptionFromPropertyName(_valuePath),
            SortMemberPath = _valuePath
        };
    }

    protected void SetupToolTip(FrameworkElementFactory elementFactory, string defaultToolTip)
    {
        if (_toolTipPath is not null)
        {
            var binding = new Binding(_toolTipPath);
            elementFactory.SetBinding(TextBlock.TextProperty, binding);
            return;
        }

        elementFactory.SetValue(FrameworkElement.ToolTipProperty, _toolTip ?? defaultToolTip);
    }

    private static DataTemplate CreateTextTemplate(Binding bindToValue)
    {
        var textFactory = new FrameworkElementFactory(typeof(TextBlock));
        textFactory.SetBinding(TextBlock.TextProperty, bindToValue);
        return new DataTemplate { VisualTree = textFactory };
    }

    private DataTemplate CreateComboEditor(Binding bindToValue)
    {
        var bindToItemsSource = new Binding(_itemsSourcePath);

        var comboFactory = new FrameworkElementFactory(typeof(ComboBox));
        comboFactory.SetValue(ItemsControl.IsTextSearchEnabledProperty, true);
        comboFactory.SetBinding(Selector.SelectedItemProperty, bindToValue);
        comboFactory.SetBinding(ItemsControl.ItemsSourceProperty, bindToItemsSource);

        return new DataTemplate { VisualTree = comboFactory };
    }
}
