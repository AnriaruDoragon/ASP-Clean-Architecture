using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Common.ApiVersioning.OpenApi;

/// <summary>
/// OpenAPI schema transformer that converts enum schemas from integer to string type
/// with camelCase values listed, matching the <see cref="JsonStringEnumConverter"/> runtime behavior.
/// </summary>
public sealed class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        Type type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;

        if (!type.IsEnum)
            return Task.CompletedTask;

        bool isNullable = schema.Type is { } t && t.HasFlag(JsonSchemaType.Null);

        schema.Type = isNullable ? JsonSchemaType.String | JsonSchemaType.Null : JsonSchemaType.String;
        schema.Format = null;

        schema.Enum = Enum.GetNames(type)
            .Select(JsonNode (name) => JsonValue.Create(JsonNamingPolicy.CamelCase.ConvertName(name)))
            .ToList();

        return Task.CompletedTask;
    }
}
