namespace Genius.Atom.Data.Persistence;

public interface IDataVersionUpgrader<in TFrom, out TTo>
    where TFrom : class
    where TTo : class
{
    TTo Upgrade(TFrom value);
}
