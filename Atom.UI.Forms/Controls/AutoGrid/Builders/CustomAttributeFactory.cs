namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public sealed class CustomAttributeFactory : IFactory<object>
{
    private readonly Type _recordType;

    public CustomAttributeFactory(Type recordType)
    {
        _recordType = recordType;
    }

    public object Create()
    {
        var factoryTypeAttr = _recordType.GetCustomAttributes(false)
            .OfType<CustomFactoryAttribute>()
            .FirstOrDefault();

        if (factoryTypeAttr is null)
        {
            return Activator.CreateInstance(_recordType).NotNull();
        }

        var builder = Module.ServiceProvider.GetService(factoryTypeAttr.FactoryType);
        if (builder is null)
        {
            throw new InvalidOperationException($"Cannot create instance of factory type {factoryTypeAttr.FactoryType}");
        }

        return builder.GetType().GetMethod(factoryTypeAttr.CreateMethod)
            !.Invoke(builder, Array.Empty<object>())
            .NotNull();
    }
}
