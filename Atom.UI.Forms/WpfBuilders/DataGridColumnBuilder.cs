using System.Windows.Controls;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.WpfBuilders;

internal class DataGridColumnBuilder
{
    protected readonly string _valuePath;
    protected StylingRecord? _cellStyling;
    private string? _title;
    private IValueConverter? _converter;
    private string? _itemsSourcePath;

    protected DataGridColumnBuilder(DataGridColumnBuilder parent)
    {
        _valuePath = parent._valuePath;
        _title = parent._title;
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

    public DataGridTextWithImageColumnBuilder WithImageSource(string imageSource)
    {
        return new DataGridTextWithImageColumnBuilder(this, imageSource);
    }

    public DataGridToggleImageButtonColumnBuilder WithToggleImageButton(string imageForTrue, string imageForFalse)
    {
        return new DataGridToggleImageButtonColumnBuilder(this, imageForTrue, imageForFalse);
    }

    public DataGridToggleSwitchColumnBuilder WithToggleSwitch()
    {
        return new DataGridToggleSwitchColumnBuilder(this);
    }

    public DataGridViewContentColumnBuilder WithViewContent(Type viewType)
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

    protected static void SetStyling(FrameworkElementFactory elementFactory, StylingRecord? styling)
    {
        if (styling is null) return;

        if (styling.HorizontalAlignment is not null)
            elementFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, styling.HorizontalAlignment);
        if (styling.Margin is not null)
            elementFactory.SetValue(FrameworkElement.MarginProperty, styling.Margin);
        if (styling.Padding is not null)
            elementFactory.SetValue(Control.PaddingProperty, styling.Padding);
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
        comboFactory.SetValue(ComboBox.IsTextSearchEnabledProperty, true);
        comboFactory.SetBinding(ComboBox.SelectedItemProperty, bindToValue);
        comboFactory.SetBinding(ComboBox.ItemsSourceProperty, bindToItemsSource);

        return new DataTemplate { VisualTree = comboFactory };
    }
}
