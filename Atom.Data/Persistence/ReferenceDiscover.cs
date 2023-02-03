using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genius.Atom.Data.Persistence;

internal sealed class ReferenceDiscoverJsonConverter : JsonConverter<object?>, IJsonConverter
{
    private readonly List<(Type, object)> _eligibleToReference = new();
    private readonly HashSet<Type> _typesToIgnore = new();

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsClass
            && typeToConvert.IsNested
            && !_typesToIgnore.Contains(typeToConvert);
    }

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        _typesToIgnore.Add(typeToConvert);

        return JsonSerializer.Deserialize(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            //writer.WriteNull();
            return;
        }

        var type = value.GetType();
        _typesToIgnore.Add(type);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.GetCustomAttribute<ReferenceAttribute>() != null);

        foreach (var property in properties)
        {
            var propertyValue = property.GetValue(value);
            if (propertyValue is not null)
            {
                _eligibleToReference.Add((property.PropertyType, propertyValue));
            }
        }

        var modifiedOptions = GetModifiedOptions(options, type);
        JsonSerializer.Serialize(writer, value, modifiedOptions);
    }

    private static JsonSerializerOptions GetModifiedOptions(JsonSerializerOptions options, Type typeToIgnore)
    {
        // WORKAROUND - stop cyclic look up
        // If we leave our converter in the options then will get infinite cycling
        // TODO: Code taken from DiscriminatedTypeConverter, and needs to be applied for this converter
        var tempOptions = new JsonSerializerOptions(options);
        var converterFactory = tempOptions.Converters.OfType<DiscriminatedTypeConverterFactory>().Single();
        var modifiedConverterFactory = DiscriminatedTypeConverterFactory.CreateWithIgnore(converterFactory, typeToIgnore);
        tempOptions.Converters.Remove(converterFactory);
        tempOptions.Converters.Add(modifiedConverterFactory);
        return tempOptions;
    }
}
