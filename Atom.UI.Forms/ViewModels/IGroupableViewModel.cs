using System.Windows.Data;

namespace Genius.Atom.UI.Forms;

public interface IGroupingField
{
    string? Label { get; }
    string? ToolTip { get; }
}

public record struct ValueGroupingField(string? Label, object? Value, IValueConverter? Converter = null, string? ToolTip = null) : IGroupingField;
public record struct CommandGroupingField(string? Label, IActionCommand Command, string? ImageName = null, string? ToolTip = null) : IGroupingField;

public interface IGroupableViewModel : IViewModel
{
    string GroupTitle { get; }
    int? ItemCount { get; }
    bool IsExpanded { get; set; }
    IEnumerable<IGroupingField> ExtraGroupFields { get; }
}
