namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

public sealed class DynamicColumnEntriesViewModel : ViewModelBase
{
    private readonly Func<IReadOnlyList<string>> _entriesRetrievalFunc;
    private IReadOnlyList<string>? _entries;

    public DynamicColumnEntriesViewModel(Func<IReadOnlyList<string>> entriesRetrievalFunc)
    {
        _entriesRetrievalFunc = entriesRetrievalFunc.NotNull();
    }

    private string? Get(int index)
    {
        if (_entries is null)
        {
            _entries = _entriesRetrievalFunc();
        }

        if (index > _entries.Count - 1)
        {
            // The entry has no values for the dynamic columns.
            return null;
        }

        return _entries[index];
    }

    public string? this[int index] => Get(index);
}
