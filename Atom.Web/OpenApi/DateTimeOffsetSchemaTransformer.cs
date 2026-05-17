using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Genius.Atom.Web.OpenApi;

/// <summary>
/// Transforms OpenAPI schemas for <see cref="DateTimeOffset" /> properties
/// from the default <c>format: "date-time"</c> (without type) to
/// <c>type: integer, format: int64</c>, so that NSwag generates <c>number</c>
/// in TypeScript, matching the actual JSON serialization as .NET ticks.
/// </summary>
public sealed class DateTimeOffsetSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        Guard.NotNull(context);
        Guard.NotNull(schema);

        var underlyingType = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type);
        var type = underlyingType ?? context.JsonTypeInfo.Type;
        if (type == typeof(DateTimeOffset))
        {
            schema.Format = "int64";

            if (underlyingType is not null) // Nullable<DateTimeOffset>
            {
                schema.Type = JsonSchemaType.Integer | JsonSchemaType.Null;
            }
            else
            {
                schema.Type = JsonSchemaType.Integer;
            }
        }

        return Task.CompletedTask;
    }
}
