namespace Genius.Atom.UI.Forms;

public sealed class DefaultGroupableViewModel : ViewModelBase, IGroupableViewModel
{
    public DefaultGroupableViewModel(string groupTitle, bool isExpanded = false)
    {
        GroupTitle = groupTitle;
        IsExpanded = isExpanded;
    }

    public string GroupTitle { get; set; }
    public int? ItemCount => null; // to be determined automatically

    public bool IsExpanded
    {
        get => GetOrDefault<bool>();
        set => RaiseAndSetIfChanged(value);
    }

    public IEnumerable<IGroupingField> ExtraGroupFields { get; } = Enumerable.Empty<IGroupingField>();
}
