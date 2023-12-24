namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

// TODO: Cover with unit tests
public sealed class DynamicColumnEntriesViewModel : ViewModelBase
{
    private readonly Func<IReadOnlyList<string>> _entriesRetrievalFunc;
    private IReadOnlyList<string>? _entries;

    public DynamicColumnEntriesViewModel(Func<IReadOnlyList<string>> entriesRetrievalFunc)
    {
        _entriesRetrievalFunc = entriesRetrievalFunc.NotNull();
    }

    private string Get(int index)
    {
        if (_entries is null)
        {
            _entries = _entriesRetrievalFunc();
        }

        if (index > _entries.Count - 1)
        {
            throw new IndexOutOfRangeException($"Entries contains {_entries.Count} items. Requested item at {index}.");
        }

        return _entries[index];
    }

    public string this[int index] => Get(index);
}
