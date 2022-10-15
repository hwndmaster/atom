namespace Genius.Atom.Infrastructure;

public interface IFactory<T>
{
    T Create();
}
