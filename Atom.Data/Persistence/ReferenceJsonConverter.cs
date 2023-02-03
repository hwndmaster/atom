using System.Text.Json;
using System.Text.Json.Serialization;
using Genius.Atom.Infrastructure.Entities;

namespace Genius.Atom.Data.Persistence;

internal sealed class ReferenceJsonConverter : JsonConverter<IEntity>, IJsonConverter
{
    private readonly ReferenceDiscoverJsonConverter _discover;
    private readonly Dictionary<Type, EntityQueryServiceProxy> _queryServices = new();

    public ReferenceJsonConverter(ReferenceDiscoverJsonConverter discover)
    {
        _discover = discover.NotNull();
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return _discover.HasType(typeToConvert);
    }

    public override IEntity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            return JsonSerializer.Deserialize(ref reader, typeToConvert, options) as IEntity;
        }

        if (!_queryServices.TryGetValue(typeToConvert, out var queryService))
        {
            queryService = EntityQueryServiceProxy.CreateForType(typeToConvert, Module.ServiceProvider);
            _queryServices.Add(typeToConvert, queryService);
        }

        var id = reader.GetGuid();
        var result = queryService.FindByIdAsync(id).GetAwaiter().GetResult();
        return result;
    }

    public override void Write(Utf8JsonWriter writer, IEntity value, JsonSerializerOptions options)
    {
        if (_discover.HasInstance(value))
        {
            writer.WriteStringValue(value.Id);
            return;
        }

        // TODO: Do a normal serialization
    }
}
