namespace Genius.Atom.UI.Forms;

#pragma warning disable S4035 // Classes implementing "IEquatable<T>" should be sealed
public class ReadOnlyTitledItemViewModel : ViewModelBase, ITitledItemViewModel, IEquatable<ReadOnlyTitledItemViewModel>
#pragma warning restore S4035 // Classes implementing "IEquatable<T>" should be sealed
{
    public ReadOnlyTitledItemViewModel(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; }

    public string Name { get; }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ReadOnlyTitledItemViewModel);
    }

    public bool Equals(ReadOnlyTitledItemViewModel? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Id.Equals(other.Id)
            && Name.Equals(other.Name);
    }

    public override int GetHashCode()
        => HashCode.Combine(Id, Name);
}
