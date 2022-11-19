namespace Genius.Atom.Data.Persistence;

internal sealed class DataVersionUpgraderProxy
{
    private readonly Func<object, object> _upgradeFunc;

    private DataVersionUpgraderProxy(Func<object, object> upgradeFunc)
    {
        _upgradeFunc = upgradeFunc;
    }

    public static DataVersionUpgraderProxy Create<TFrom, TTo>(IDataVersionUpgrader<TFrom, TTo> dataVersionUpgrader)
        where TFrom : class
        where TTo : class
    {
        return new DataVersionUpgraderProxy(x => dataVersionUpgrader.Upgrade((TFrom)x));
    }

    public object Upgrade(object value)
    {
        return _upgradeFunc(value);
    }
}
