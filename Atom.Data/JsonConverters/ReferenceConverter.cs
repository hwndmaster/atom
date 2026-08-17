using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genius.Atom.Data.JsonConverters;

/// <summary>
/// Serializes <see cref="int"/>-keyed references as plain JSON numbers.
/// See <see cref="GuidReferenceConverter{TReference}"/> for <see cref="Guid"/>-keyed references.
/// </summary>
/// <typeparam name="TReference">The strongly-typed reference.</typeparam>
public sealed class ReferenceConverter<TReference> : JsonConverter<TReference>
    where TReference : IReference<int, TReference>
{
    public override TReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var id = reader.GetInt32();
            return TReference.Create(id);
        }

        throw new JsonException("Expected a numeric ID for reference.");
    }

    public override void Write(Utf8JsonWriter writer, TReference value, JsonSerializerOptions options)
    {
        Guard.NotNull(writer);
        writer.WriteNumberValue(value.Id);
    }
}

/// <summary>
/// Serializes <see cref="Guid"/>-keyed references as plain JSON strings.
/// See <see cref="ReferenceConverter{TReference}"/> for <see cref="int"/>-keyed references.
/// </summary>
/// <typeparam name="TReference">The strongly-typed reference.</typeparam>
public sealed class GuidReferenceConverter<TReference> : JsonConverter<TReference>
    where TReference : IReference<Guid, TReference>
{
    public override TReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return TReference.Create(reader.GetGuid());
        }

        throw new JsonException("Expected a GUID string for reference.");
    }

    public override void Write(Utf8JsonWriter writer, TReference value, JsonSerializerOptions options)
    {
        Guard.NotNull(writer);
        writer.WriteStringValue(value.Id);
    }
}
