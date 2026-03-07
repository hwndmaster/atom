using System.Collections.Concurrent;
using Genius.Atom.Infrastructure;

namespace Genius.Atom.UI.Forms.TestingUtil;

public class TestWpfApplication : IWpfApplication
{
    private readonly ConcurrentDictionary<string, object> _resources = [];

    public TestWpfApplication AddSampleResources<T>(string resourceName, T resource)
    {
        _resources.TryAdd(resourceName, resource.NotNull());
        return this;
    }

    public T FindResource<T>(string resourceName)
    {
        return (T)_resources[resourceName];
    }
}
