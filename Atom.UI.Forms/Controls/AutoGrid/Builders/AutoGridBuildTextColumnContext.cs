using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public record IconSourceRecord(string IconPropertyPath, double? FixedSize = null, bool HideText = false);

public sealed class AutoGridBuildTextColumnContext : AutoGridBuildColumnContext
{
    public AutoGridBuildTextColumnContext(PropertyDescriptor property, string displayName)
        : base(property, displayName)
    {
    }

    public string? DisplayFormat { get; init; }
    public string? TextHighlightingPatternPath { get; init; }
    public string? TextHighlightingUseRegexPath { get; init; }
    public bool IsGrouped { get; init; }
    public bool Filterable { get; init; }
    public IconSourceRecord? IconSource { get; init; }
}
