using Genius.Atom.Data;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Genius.Atom.Web.OpenApi;

/// <summary>
/// Transforms OpenAPI parameter schemas for Reference types (e.g. CategoryRef, ProductRef)
/// from <c>string</c> to <c>integer</c> so that NSwag generates the proper TypeScript
/// reference types instead of <c>string</c>.
/// </summary>
public sealed class ReferenceParameterTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        Guard.NotNull(context);
        Guard.NotNull(operation);

        var methodParams = context.Description.ActionDescriptor
            is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor cad
            ? cad.MethodInfo.GetParameters()
            : [];

        foreach (var paramInterface in operation.Parameters ?? [])
        {
            if (paramInterface is not OpenApiParameter param)
                continue;

            var methodParam = methodParams
                .FirstOrDefault(p => string.Equals(p.Name, param.Name, StringComparison.OrdinalIgnoreCase));
            if (methodParam == null)
                continue;

            var referenceType = methodParam.ParameterType.GetInterfaces().FirstOrDefault(x =>
                x.IsGenericType
                && x.GetGenericTypeDefinition() == typeof(IReference<,>));
            if (referenceType is null)
                continue;

            if (referenceType.GenericTypeArguments[0] == typeof(int))
            {
                param.Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Integer,
                    Format = "int32",
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = methodParam.ParameterType.Name,
                    }
                };
            }

            if (referenceType.GenericTypeArguments[0] == typeof(Guid))
            {
                param.Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Format = "uuid",
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = methodParam.ParameterType.Name,
                    }
                };
            }

            if (referenceType.GenericTypeArguments[0] == typeof(string))
            {
                param.Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = methodParam.ParameterType.Name,
                    }
                };
            }
        }

        return Task.CompletedTask;
    }
}
