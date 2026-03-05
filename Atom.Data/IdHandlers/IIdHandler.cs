namespace Genius.Atom.Data.IdHandlers;

public interface IIdHandler<TEntityKey>
    where TEntityKey : notnull
{
    bool IsDefault(TEntityKey id);
    TEntityKey GenerateId();
}
