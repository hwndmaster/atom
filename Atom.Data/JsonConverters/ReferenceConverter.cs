using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genius.Atom.Data.JsonConverters;

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
