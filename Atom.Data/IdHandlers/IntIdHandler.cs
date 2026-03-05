namespace Genius.Atom.Data.IdHandlers;

public sealed class IntIdHandler : IIdHandler<int>
{
    public bool IsDefault(int id) => id == 0;

    public int GenerateId()
    {
        var timestamp = (int)(DateTimeOffset.UtcNow.Ticks % int.MaxValue);
        var guidHash = Guid.NewGuid().GetHashCode();
        return timestamp ^ guidHash;
    }
}
