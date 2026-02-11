using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Common.ApiVersioning.OpenApi;

/// <summary>
/// Fixes a .NET 10 issue where ASP.NET Core OpenAPI generation sets multi-type flags
/// (e.g., <c>Integer | String</c>) on numeric properties due to JSON Schema 2020-12 semantics.
/// OpenAPI 3.0 only supports a single <c>type</c> value, so the serializer drops the field
/// entirely when multiple non-null types are present — causing Scalar UI to display these as strings.
/// </summary>
/// <remarks>
/// .NET 10's schema generator includes <see cref="JsonSchemaType.String"/> alongside the numeric type
/// because the <c>pattern</c> regex applies to string representations. This transformer strips the
/// <c>String</c> flag, preserving only the numeric type (plus <c>Null</c> for nullable properties
/// which OpenAPI 3.0 renders as <c>nullable: true</c>).
/// </remarks>
public sealed class NumericTypeSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Fix the schema itself (e.g., a standalone int parameter)
        FixNumericType(schema);

        // Fix properties within this schema (e.g., int fields on a DTO)
        if (schema.Properties is not null)
        {
            foreach ((string _, IOpenApiSchema propertySchema) in schema.Properties)
            {
                switch (propertySchema)
                {
                    case OpenApiSchema concreteSchema:
                        FixNumericType(concreteSchema);
                        break;
                    case OpenApiSchemaReference { Target: OpenApiSchema targetSchema }:
                        FixNumericType(targetSchema);
                        break;
                }
            }
        }

        // Fix array item schemas (e.g., List<int>)
        switch (schema.Items)
        {
            case OpenApiSchema itemsSchema:
                FixNumericType(itemsSchema);
                break;
            case OpenApiSchemaReference { Target: OpenApiSchema targetItemsSchema }:
                FixNumericType(targetItemsSchema);
                break;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Strips the <see cref="JsonSchemaType.String"/> flag from numeric type schemas,
    /// leaving only the correct numeric type (and <see cref="JsonSchemaType.Null"/> if nullable).
    /// </summary>
    private static void FixNumericType(OpenApiSchema schema)
    {
        if (schema.Type is not { } type || schema.Format is null)
            return;

        // Determine the expected numeric type from the format
        JsonSchemaType? numericType = schema.Format switch
        {
            "int32" or "int64" => JsonSchemaType.Integer,
            "float" or "double" or "decimal" => JsonSchemaType.Number,
            _ => null
        };

        if (numericType is null)
            return;

        // Only fix if the current type includes String alongside the numeric type
        // (e.g., Integer | String or Null | Integer | String)
        if (!type.HasFlag(JsonSchemaType.String) || !type.HasFlag(numericType.Value))
            return;

        // Keep only the numeric type + Null flag (for nullable properties)
        schema.Type = type.HasFlag(JsonSchemaType.Null)
            ? numericType.Value | JsonSchemaType.Null
            : numericType.Value;
    }
}
