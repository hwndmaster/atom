using Genius.Atom.Infrastructure;

namespace Genius.Atom.UI.Forms.TestingUtil;

public class TestWpfApplication : IWpfApplication
{
    private readonly Dictionary<string, object> _resources = [];

    public TestWpfApplication AddSampleResources<T>(string resourceName, T resource)
    {
        if (!_resources.ContainsKey(resourceName))
        {
            _resources.Add(resourceName, resource.NotNull());
        }
        return this;
    }

    public T FindResource<T>(string resourceName)
    {
        return (T)_resources[resourceName];
    }
}
