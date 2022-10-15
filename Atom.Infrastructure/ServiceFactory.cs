using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Infrastructure;

public sealed class ServiceFactory<T> : IFactory<T>
{
    public T Create()
    {
        return Module.ServiceProvider.GetService<T>()
            ?? Activator.CreateInstance<T>();
    }
}
