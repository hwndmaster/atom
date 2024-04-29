using System.Diagnostics.CodeAnalysis;

namespace Genius.Atom.UI.Forms;

public interface IWpfApplication
{
    T FindResource<T>(string resourceName);
}

[ExcludeFromCodeCoverage]
internal sealed class WpfApplication : IWpfApplication
{
    private readonly Application _application;

    public WpfApplication(Application application)
    {
        _application = application.NotNull();
    }

    public T FindResource<T>(string resourceName)
    {
        var resource = _application.FindResource(resourceName)
            ?? throw new ResourceReferenceKeyNotFoundException("Resource was not found: " + resourceName, resourceName);

        return (T)resource;
    }
}
