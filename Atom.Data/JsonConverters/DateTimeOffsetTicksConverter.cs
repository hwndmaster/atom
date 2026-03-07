using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genius.Atom.Data.JsonConverters;

internal sealed class DateTimeOffsetTicksConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var ticks = reader.GetString() ?? string.Empty;
            if (string.IsNullOrEmpty(ticks))
            {
                ticks = "0";
            }
            return new DateTimeOffset(long.Parse(ticks, CultureInfo.InvariantCulture), TimeSpan.Zero);
        }

        throw new JsonException("Expected string with DateTimeOffset ticks.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        Guard.NotNull(writer);
        writer.WriteStringValue(value.UtcTicks.ToString(CultureInfo.InvariantCulture));
    }
}
