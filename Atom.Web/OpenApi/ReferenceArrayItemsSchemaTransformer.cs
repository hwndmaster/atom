using Genius.Atom.Data;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Genius.Atom.Web.OpenApi;

/// <summary>
/// Transforms OpenAPI array schemas whose element type is a Reference type
/// (e.g. <c>RecordPhotoRef[]</c>) by populating the missing <c>items</c> with a
/// <c>$ref</c> to the element's component schema.
/// <para>
/// The .NET OpenAPI generator emits such arrays as a bare <c>{ "type": "array" }</c>
/// without an <c>items</c> schema (because Reference types carry a custom
/// <c>JsonConverter</c> that serializes them as a scalar), which makes NSwag
/// generate <c>any[]</c> instead of e.g. <c>RecordPhotoRef[]</c>. Scalar Reference
/// properties already get a proper <c>$ref</c>; this restores the same for arrays.
/// </para>
/// </summary>
public sealed class ReferenceArrayItemsSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        Guard.NotNull(context);
        Guard.NotNull(schema);

        if (schema.Type is not { } schemaType
            || !schemaType.HasFlag(JsonSchemaType.Array)
            || schema.Items is not null)
        {
            return;
        }

        var elementType = TryGetEnumerableElementType(context.JsonTypeInfo.Type);
        if (elementType is null || !ImplementsReference(elementType))
        {
            return;
        }

        // The generator already emits a component schema named after the Reference
        // type (the same one scalar Reference properties $ref). Resolve it via the
        // pipeline so the array's items point at the strongly-typed Reference
        // instead of being dropped (which NSwag would otherwise render as any[]).
        schema.Items = await context.GetOrCreateSchemaAsync(elementType, null, cancellationToken);
    }

    private static bool ImplementsReference(Type type)
        => type.GetInterfaces().Any(x =>
            x.IsGenericType
            && (x.GetGenericTypeDefinition() == typeof(IReference<,>)
                || x.GetGenericTypeDefinition() == typeof(IReference<>)));

    private static Type? TryGetEnumerableElementType(Type type)
    {
        if (type == typeof(string))
        {
            return null;
        }

        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return type.GetGenericArguments()[0];
        }

        Type? enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableInterface?.GetGenericArguments()[0];
    }
}
