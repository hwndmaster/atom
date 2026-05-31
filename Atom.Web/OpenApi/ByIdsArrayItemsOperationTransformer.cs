using Genius.Atom.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Genius.Atom.Web.Controllers;

namespace Genius.Atom.Web.OpenApi;

/// <summary>
/// Fixes request body array schemas missing <c>items</c> and assigns stable operation IDs for by-ids endpoints.
/// Useful for <see cref="BaseCrudController{TKey, TReference, TData, TRepository, TCreateRequest, TUpdateRequest}.GetByIds(IEnumerable{TReference}, CancellationToken)"/> endpoint.
/// </summary>
public sealed class ByIdsArrayItemsOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        Guard.NotNull(operation);
        Guard.NotNull(context);

        SetByIdsOperationId(operation, context);

        if (operation.RequestBody is null
            || context.Description is null)
        {
            return Task.CompletedTask;
        }

        IDictionary<string, OpenApiMediaType>? requestBodyContent = operation.RequestBody.Content;
        if (requestBodyContent is null)
        {
            return Task.CompletedTask;
        }

        Type? bodyType = context.Description.ParameterDescriptions
            .FirstOrDefault(parameter => parameter.Source == BindingSource.Body)
            ?.Type;

        if (bodyType is null)
        {
            return Task.CompletedTask;
        }

        Type? elementType = TryGetEnumerableElementType(bodyType);
        if (elementType is null)
        {
            return Task.CompletedTask;
        }

        (JsonSchemaType type, string? format)? schemaType = GetOpenApiScalarType(elementType);
        if (schemaType is null)
        {
            return Task.CompletedTask;
        }

        foreach (OpenApiMediaType mediaType in requestBodyContent.Values)
        {
            if (mediaType.Schema is not { Type: JsonSchemaType.Array, Items: null } existingSchema)
            {
                continue;
            }

            if (existingSchema is OpenApiSchema concreteSchema)
            {
                concreteSchema.Items = new OpenApiSchema
                {
                    Type = schemaType.Value.type,
                    Format = schemaType.Value.format,
                };

                continue;
            }

            mediaType.Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.Array,
                Items = new OpenApiSchema
                {
                    Type = schemaType.Value.type,
                    Format = schemaType.Value.format,
                },
            };
        }

        return Task.CompletedTask;
    }

    private static void SetByIdsOperationId(OpenApiOperation operation, OpenApiOperationTransformerContext context)
    {
        if (!string.IsNullOrWhiteSpace(operation.OperationId))
        {
            return;
        }

        string? relativePath = context.Description.RelativePath;
        if (string.IsNullOrWhiteSpace(relativePath)
            || !relativePath.EndsWith("/by-ids", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string controllerName = context.Description.ActionDescriptor.RouteValues.TryGetValue("controller", out string? name)
            && !string.IsNullOrWhiteSpace(name)
            ? name
            : "Api";

        operation.OperationId = $"{controllerName}_byIds";
    }

    private static Type? TryGetEnumerableElementType(Type type)
    {
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

    private static (JsonSchemaType type, string? format)? GetOpenApiScalarType(Type type)
    {
        Type referenceValueType = TryGetReferenceValueType(type) ?? type;
        Type scalarType = Nullable.GetUnderlyingType(referenceValueType) ?? referenceValueType;

        return scalarType switch
        {
            var t when t == typeof(int) => (JsonSchemaType.Integer, "int32"),
            var t when t == typeof(long) => (JsonSchemaType.Integer, "int64"),
            var t when t == typeof(short) => (JsonSchemaType.Integer, "int32"),
            var t when t == typeof(byte) => (JsonSchemaType.Integer, "int32"),
            var t when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => (JsonSchemaType.Number, null),
            var t when t == typeof(bool) => (JsonSchemaType.Boolean, null),
            var t when t == typeof(Guid) => (JsonSchemaType.String, "uuid"),
            var t when t == typeof(DateOnly) => (JsonSchemaType.String, "date"),
            var t when t == typeof(DateTime) || t == typeof(DateTimeOffset) => (JsonSchemaType.String, "date-time"),
            var t when t == typeof(string) => (JsonSchemaType.String, null),
            { IsEnum: true } => (JsonSchemaType.String, null),
            _ => null,
        };
    }

    private static Type? TryGetReferenceValueType(Type type)
    {
        Type? referenceInterface = type.GetInterfaces()
            .FirstOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IReference<,>));

        if (referenceInterface is not null)
        {
            return referenceInterface.GetGenericArguments()[0];
        }

        Type? singleReferenceInterface = type.GetInterfaces()
            .FirstOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IReference<>));

        return singleReferenceInterface?.GetGenericArguments()[0];
    }
}
