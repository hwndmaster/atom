using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genius.Atom.Data.Persistence;

internal sealed class DiscriminatedTypeConverterFactory : JsonConverterFactory
{
    private readonly ITypeDiscriminators _typeDiscriminators;
    private readonly Type? _typeToIgnore;

    public DiscriminatedTypeConverterFactory(ITypeDiscriminators typeDiscriminators)
    {
        _typeDiscriminators = typeDiscriminators.NotNull();
    }

    private DiscriminatedTypeConverterFactory(ITypeDiscriminators typeDiscriminators, Type typeToIgnore)
        : this(typeDiscriminators)
    {
        _typeToIgnore = typeToIgnore;
    }

    public static DiscriminatedTypeConverterFactory CreateWithIgnore(DiscriminatedTypeConverterFactory other, Type typeToIgnore)
    {
        return new DiscriminatedTypeConverterFactory(other._typeDiscriminators, typeToIgnore);
    }

    public override bool CanConvert(Type typeToConvert)
    {
        if (_typeToIgnore == typeToConvert)
        {
            return false;
        }

        return _typeDiscriminators.HasMapping(typeToConvert);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var concreteConverterType = typeof(DiscriminatedTypeConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(concreteConverterType, _typeDiscriminators).NotNull();
    }
}

internal sealed class DiscriminatedTypeConverter<T> : JsonConverter<T>, IJsonConverter
{
    private const string DiscriminatorProperty = "$type";
    private const string VersionProperty = "$v";

    private readonly ITypeDiscriminatorsInternal _typeDiscriminators;
    private readonly JsonEncodedText DiscriminatorPropertyEncoded = JsonEncodedText.Encode(DiscriminatorProperty);
    private readonly JsonEncodedText VersionPropertyEncoded = JsonEncodedText.Encode(VersionProperty);

    public DiscriminatedTypeConverter(ITypeDiscriminatorsInternal typeDiscriminators)
    {
        _typeDiscriminators = typeDiscriminators.NotNull();
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(T).IsAssignableFrom(typeToConvert);
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Creating a copy of the reader (The derived deserialization has to be done from the start)
        Utf8JsonReader typeReader = reader;

        if (typeReader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        while (typeReader.Read())
        {
            if (typeReader.TokenType == JsonTokenType.PropertyName &&
                typeReader.ValueTextEquals(DiscriminatorPropertyEncoded.EncodedUtf8Bytes))
            {
                typeReader.Read();
                if (typeReader.TokenType != JsonTokenType.String)
                {
                    throw new InvalidOperationException($"Expected string discriminator value, got '{reader.TokenType}'");
                }
                var discriminator = typeReader.GetString().NotNull();
                var version = 1;

                typeReader.Read();
                if (typeReader.TokenType == JsonTokenType.PropertyName &&
                    typeReader.ValueTextEquals(VersionPropertyEncoded.EncodedUtf8Bytes))
                {
                    typeReader.Read();
                    if (typeReader.TokenType != JsonTokenType.Number)
                    {
                        throw new InvalidOperationException($"Expected number version value, got '{reader.TokenType}'");
                    }
                    version = typeReader.GetInt32();
                }

                var discriminatorRecord = _typeDiscriminators.GetDiscriminator(discriminator, version);

                var modifiedOptions = GetModifiedOptions(options, discriminatorRecord.Type);
                var result = JsonSerializer.Deserialize(ref reader, discriminatorRecord.Type, modifiedOptions).NotNull();

                result = UpgradeVersionIfNeeded(result, discriminatorRecord);

                return (T)result;
            }
            else if (typeReader.TokenType == JsonTokenType.StartObject || typeReader.TokenType == JsonTokenType.StartArray)
            {
                if (!typeReader.TrySkip())
                {
                    typeReader.Skip();
                }
            }
        }

        throw new InvalidOperationException($"Object has no discriminator '{DiscriminatorProperty}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            return;
        }

        var valueType = value.GetType();
        var discriminatorRecord = _typeDiscriminators.GetDiscriminator(valueType);

        writer.WriteStartObject();
        writer.WriteString(DiscriminatorProperty, discriminatorRecord.Discriminator);
        writer.WriteNumber(VersionProperty, discriminatorRecord.Version);

        var modifiedOptions = GetModifiedOptions(options, valueType);
        using (var document = JsonSerializer.SerializeToDocument(value, valueType, modifiedOptions))
        {
            foreach (var property in document.RootElement.EnumerateObject())
            {
                property.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }

    private static JsonSerializerOptions GetModifiedOptions(JsonSerializerOptions options, Type typeToIgnore)
    {
        // WORKAROUND - stop cyclic look up
        // If we leave our converter in the options then will get infinite cycling
        var tempOptions = new JsonSerializerOptions(options);
        var converterFactory = tempOptions.Converters.OfType<DiscriminatedTypeConverterFactory>().Single();
        var modifiedConverterFactory = DiscriminatedTypeConverterFactory.CreateWithIgnore(converterFactory, typeToIgnore);
        tempOptions.Converters.Remove(converterFactory);
        tempOptions.Converters.Add(modifiedConverterFactory);
        return tempOptions;
    }

    private object UpgradeVersionIfNeeded(object value, TypeDiscriminatorRecord fromVersionRecord)
    {
        var previousVersionRecord = fromVersionRecord;
        while (true)
        {
            var nextVersionRecord = _typeDiscriminators.GetDiscriminatorByPreviousVersion(previousVersionRecord.Type);
            if (nextVersionRecord is null || nextVersionRecord.VersionUpgrader is null)
            {
                break;
            }
            value = nextVersionRecord.VersionUpgrader.Upgrade(value);
            previousVersionRecord = nextVersionRecord;
        }

        return value;
    }
}
