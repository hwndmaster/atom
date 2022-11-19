namespace Genius.Atom.Data.Persistence;

public interface IDataVersionUpgrader<TFrom, TTo>
    where TFrom : class
    where TTo : class
{
    TTo Upgrade(TFrom value);
}
