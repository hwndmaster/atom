using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Wpf;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal sealed class DefaultAutoGridBuilder
{
    private Type? _recordType;

    internal DefaultAutoGridBuilder ForType(Type recordType)
    {
        _recordType = recordType;
        return this;
    }

    public AutoGridBuildContext Build()
    {
        Guard.NotNull(_recordType);

        var properties = TypeDescriptor.GetProperties(_recordType);

        var columns = properties.OfType<PropertyDescriptor>()
            .Where(pd => IsBrowsable(_recordType, pd))
            .Select(pd => CreateColumnContext(pd));

        var recordFactory = new CustomAttributeFactory(_recordType);

        return new AutoGridBuildContext(columns, recordFactory);
    }

    private AutoGridBuildColumnContext CreateColumnContext(PropertyDescriptor property)
    {
        // TODO: Check if `property.DisplayName` is correct
        var displayName = property.Attributes.OfType<TitleAttribute>().FirstOrDefault()?.Title
            ?? Regex.Replace(property.DisplayName, "[A-Z]", " $0");

        if (AutoGridBuilderHelpers.IsCommandColumn(property))
        {
            return new AutoGridBuildCommandColumnContext(property, displayName)
            {
                AutoWidth = DetectAutoWidth(property),
                Icon = DetectIcon(property),
                IsReadOnly = DetectIsReadOnly(property),
                Style = DetectStyle(property),
                ToolTipPath = DetectToolTipPath(property),
                ValueConverter = DetectValueConverter(property, null)
            };
        }
        else if (property.Attributes.OfType<SelectFromListAttribute>().Any())
        {
            var selectFromListAttr = property.Attributes.OfType<SelectFromListAttribute>().First();
            return new AutoGridBuildComboBoxColumnContext(property, displayName,
                selectFromListAttr.CollectionPropertyName, selectFromListAttr.FromOwnerContext)
            {
                Style = DetectStyle(property)
            };
        }
        else if (property.Attributes.OfType<AttachedViewAttribute>().Any())
        {
            var attachedViewAttr = property.Attributes.OfType<AttachedViewAttribute>().First();
            return new AutoGridBuildViewColumnContext(property, displayName, attachedViewAttr.AttachedViewType)
            {
                AutoWidth = DetectAutoWidth(property),
                IsReadOnly = DetectIsReadOnly(property),
                Style = DetectStyle(property)
            };
        }
        else
        {
            var displayFormat = DetectDisplayFormat(property);
            return new AutoGridBuildTextColumnContext(property, displayName)
            {
                AutoWidth = DetectAutoWidth(property),
                DisplayFormat = displayFormat,
                IconSource = DetectIconSource(property),
                IsGrouped = DetectIsGrouped(property),
                IsReadOnly = DetectIsReadOnly(property),
                Filterable = DetectFilterable(property),
                Style = DetectStyle(property),
                ToolTipPath = DetectToolTipPath(property),
                ValueConverter = DetectValueConverter(property, displayFormat)
            };
        }
    }

    private IValueConverter? DetectValueConverter(PropertyDescriptor property, string? displayFormat)
    {
        var converterAttr = property.Attributes.OfType<ValueConverterAttribute>().FirstOrDefault();
        if (converterAttr is not null)
        {
            var instance = Activator.CreateInstance(converterAttr.ValueConverterType) as IValueConverter;
            return instance.NotNull(nameof(instance));
        }

        if (converterAttr is null && !property.PropertyType.IsValueType)
        {
            return new PropertyValueStringConverter(displayFormat);
        }

        return null;
    }

    private static bool IsBrowsable(Type recordType, PropertyDescriptor property)
    {
        var showOnlyBrowsable = recordType.GetCustomAttributes(false)
            .Any(x => x is ShowOnlyBrowsableAttribute b && b.OnlyBrowsable);

        var browsable = property.Attributes.OfType<BrowsableAttribute>().FirstOrDefault();
        return (!showOnlyBrowsable || browsable?.Browsable == true)
            && (showOnlyBrowsable || browsable?.Browsable != false);
    }

    private static bool DetectAutoWidth(PropertyDescriptor property)
        => property.Attributes.OfType<GreedyAttribute>().Any();

    private static string? DetectDisplayFormat(PropertyDescriptor property)
        => property.Attributes.OfType<DisplayFormatAttribute>().FirstOrDefault()?.DataFormatString;

    private static bool DetectIsGrouped(PropertyDescriptor property)
        => property.Attributes.OfType<GroupByAttribute>().Any();

    private static bool DetectIsReadOnly(PropertyDescriptor property)
        => property.Attributes.OfType<ReadOnlyAttribute>().FirstOrDefault()?.IsReadOnly == true;

    private static bool DetectFilterable(PropertyDescriptor property)
        => property.Attributes.OfType<FilterByAttribute>().Any();

    private static string? DetectIcon(PropertyDescriptor property)
        => property.Attributes.OfType<IconAttribute>().FirstOrDefault()?.Name;

    private static string? DetectToolTipPath(PropertyDescriptor property)
        => property.Attributes.OfType<TooltipSourceAttribute>().FirstOrDefault()?.Path;

    private static IconSourceRecord? DetectIconSource(PropertyDescriptor property)
    {
        var attr = property.Attributes.OfType<IconSourceAttribute>().FirstOrDefault();
        if (attr is null)
        {
            return null;
        }

        return new IconSourceRecord(attr.IconPropertyPath, attr.FixedSize, attr.HideText);
    }

    private static StylingRecord? DetectStyle(PropertyDescriptor property)
    {
        var attr = property.Attributes.OfType<StyleAttribute>().FirstOrDefault();
        if (attr is null)
        {
            return null;
        }

        return new StylingRecord(attr.HorizontalAlignment);
    }
}
