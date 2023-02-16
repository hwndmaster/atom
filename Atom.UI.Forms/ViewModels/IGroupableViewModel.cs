using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms;

public interface IGroupingField { }

public record struct ValueGroupingField(string? Label, object? Value, IValueConverter? Converter) : IGroupingField;
public record struct CommandGroupingField(string? Label, string? ImageName, IActionCommand Command) : IGroupingField;

public interface IGroupableViewModel : IViewModel
{
    string GroupTitle { get; }
    int ItemCount { get; }
    bool IsExpanded { get; set; }
    IEnumerable<IGroupingField> ExtraGroupFields { get; }
}
