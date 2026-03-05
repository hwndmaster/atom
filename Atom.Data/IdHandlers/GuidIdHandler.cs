namespace Genius.Atom.Data.IdHandlers;

public sealed class GuidIdHandler : IIdHandler<Guid>
{
    public bool IsDefault(Guid id) => id == Guid.Empty;

    public Guid GenerateId() => Guid.NewGuid();
}
