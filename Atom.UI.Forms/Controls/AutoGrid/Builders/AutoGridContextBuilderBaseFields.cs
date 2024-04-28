using System.Windows.Data;
using Genius.Atom.UI.Forms.Wpf;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal record AutoGridContextBuilderBaseFields(
    bool AutoWidth,
    string? DisplayName,
    bool IsReadOnly,
    StylingRecord? Style,
    string? ToolTip,
    string? ToolTipPath,
    IValueConverter? ValueConverter,
    string? VisibilityBinding)
{
    public static readonly AutoGridContextBuilderBaseFields Default = new(false, null, false, null, null, null, null, null);
}
