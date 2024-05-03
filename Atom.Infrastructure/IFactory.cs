namespace Genius.Atom.Infrastructure;

public interface IFactory<out T>
{
    T Create();
}
