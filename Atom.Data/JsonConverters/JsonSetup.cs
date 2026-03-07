using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genius.Atom.Data.JsonConverters;

public static class JsonSetup
{
    public static void SetupJsonOptions(JsonSerializerOptions options)
    {
        Guard.NotNull(options);

        options.Converters.Add(new DateTimeOffsetTicksConverter());
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    }
}
