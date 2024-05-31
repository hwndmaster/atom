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

    private static AutoGridBuildColumnContext CreateColumnContext(PropertyDescriptor property)
    {
        var displayName = property.Attributes.OfType<TitleAttribute>().FirstOrDefault()?.Title
            ?? Regex.Replace(property.DisplayName, "[A-Z]", " $0");

        if (AutoGridBuilderHelpers.IsCommandColumn(property))
        {
            var baseFields = new AutoGridContextBuilderBaseFields(
                DetectAutoWidth(property),
                displayName,
                DetectIsReadOnly(property),
                DetectStyle(property),
                null,
                DetectToolTipPath(property),
                DetectValueConverter(property, null),
                null
            );
            return new AutoGridBuildCommandColumnContext(property, baseFields)
            {
                Icon = DetectIcon(property)
            };
        }
        else if (property.Attributes.OfType<SelectFromListAttribute>().Any())
        {
            var selectFromListAttr = property.Attributes.OfType<SelectFromListAttribute>().First();
            var baseFields = new AutoGridContextBuilderBaseFields(
                false,
                displayName,
                false,
                DetectStyle(property),
                null,
                null,
                null,
                null
            );
            return new AutoGridBuildComboBoxColumnContext(property, baseFields)
            {
                CollectionPropertyName = selectFromListAttr.CollectionPropertyName,
                FromOwnerContext = selectFromListAttr.FromOwnerContext
            };
        }
        else if (property.Attributes.OfType<AttachedViewAttribute>().Any())
        {
            var attachedViewAttr = property.Attributes.OfType<AttachedViewAttribute>().First();
            var baseFields = new AutoGridContextBuilderBaseFields(
                DetectAutoWidth(property),
                displayName,
                DetectIsReadOnly(property),
                DetectStyle(property),
                null,
                null,
                null,
                null
            );
            return new AutoGridBuildViewColumnContext(property, baseFields)
            {
                AttachedViewType = attachedViewAttr.AttachedViewType
            };
        }
        else
        {
            var displayFormat = DetectDisplayFormat(property);
            var baseFields = new AutoGridContextBuilderBaseFields(
                DetectAutoWidth(property),
                displayName,
                DetectIsReadOnly(property),
                DetectStyle(property),
                null,
                DetectToolTipPath(property),
                DetectValueConverter(property, displayFormat),
                null
            );
            return new AutoGridBuildTextColumnContext(property, baseFields)
            {
                DisplayFormat = displayFormat,
                IconSource = DetectIconSource(property),
                IsGrouped = DetectIsGrouped(property),
                Filterable = DetectFilterable(property),
            };
        }
    }

    private static IValueConverter? DetectValueConverter(PropertyDescriptor property, string? displayFormat)
    {
        var converterAttr = property.Attributes.OfType<ValueConverterAttribute>().FirstOrDefault();
        if (converterAttr is not null)
        {
            var instance = Activator.CreateInstance(converterAttr.ValueConverterType) as IValueConverter;
            return instance.NotNull();
        }

        if (!property.PropertyType.IsValueType)
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
