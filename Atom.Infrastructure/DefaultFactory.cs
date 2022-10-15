namespace Genius.Atom.Infrastructure;

public sealed class DefaultFactory<T> : IFactory<T>
{
    private readonly Func<T> _factoryFunc;

    public DefaultFactory(Func<T> factoryFunc)
    {
        _factoryFunc = factoryFunc.NotNull();
    }

    public T Create()
    {
        return _factoryFunc();
    }
}
