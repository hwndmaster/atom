using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Wpf;

internal static class StylingHelpers
{
    internal static void CopyStyle(Style styleFrom, Style styleTo)
    {
        foreach (var resourceKey in styleFrom.Resources.Keys)
        {
            styleTo.Resources.Add(resourceKey, styleFrom.Resources[resourceKey]);
        }
        foreach (var setter in styleFrom.Setters)
        {
            styleTo.Setters.Add(setter);
        }
        foreach (var trigger in styleFrom.Triggers)
        {
            styleTo.Triggers.Add(trigger);
        }
    }

    internal static Style EnsureDefaultCellStyle(DataGridColumn column)
    {
        if (column.CellStyle is null)
        {
            column.CellStyle = new Style {
                TargetType = typeof(DataGridCell),
                BasedOn = (Style) Application.Current.FindResource("MahApps.Styles.DataGridCell")
            };
        }

        return column.CellStyle;
    }

    internal static void SetStyling(Style style, StylingRecord? styling)
    {
        if (styling is null) return;

        foreach (var setter in CreateSetters(styling))
        {
            style.Setters.Add(setter);
        }
    }

    internal static void SetStyling(FrameworkElementFactory elementFactory, StylingRecord? styling)
    {
        if (styling is null) return;

        foreach (var setter in CreateSetters(styling))
        {
            elementFactory.SetValue(setter.Property, setter.Value);
        }
    }

    private static IEnumerable<Setter> CreateSetters(StylingRecord styling)
    {
        if (styling.HorizontalAlignment is not null)
            yield return new Setter(FrameworkElement.HorizontalAlignmentProperty, styling.HorizontalAlignment);
        if (styling.Margin is not null)
            yield return new Setter(FrameworkElement.MarginProperty, styling.Margin);
        if (styling.Padding is not null)
            yield return new Setter(Control.PaddingProperty, styling.Padding);
    }
}
