using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Infrastructure;

public sealed class Lazier<T> : Lazy<T> where T : class
{
    public Lazier(IServiceProvider provider)
        : base(() => provider.GetRequiredService<T>())
    {
    }
}
