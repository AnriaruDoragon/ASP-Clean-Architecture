using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Common.ApiVersioning.OpenApi;

/// <summary>
/// OpenAPI operation transformer that extracts FluentValidation rules and applies them
/// to query and route parameters. Complements <see cref="FluentValidationSchemaTransformer"/>
/// which only handles request body schemas.
/// </summary>
/// <remarks>
/// <para>
/// Works by inspecting <see cref="Microsoft.AspNetCore.Mvc.ApiExplorer.ApiParameterDescription.ModelMetadata"/>
/// to find the container type (e.g., a query record bound via <c>[FromQuery]</c>), then resolving
/// the FluentValidation validator for that type and applying rules to the matching OpenAPI parameters.
/// </para>
/// <para>
/// Controller actions should bind query types directly for this to work:
/// <code>
/// public async Task&lt;IActionResult&gt; GetProducts(
///     [FromQuery] GetProductsQuery query,
///     CancellationToken cancellationToken)
/// </code>
/// </para>
/// </remarks>
public sealed class FluentValidationOperationTransformer(IServiceProvider serviceProvider)
    : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (operation.Parameters is null || operation.Parameters.Count == 0)
            return Task.CompletedTask;

        using IServiceScope scope = serviceProvider.CreateScope();

        // Group API parameter descriptions by their container type
        IEnumerable<IGrouping<Type, ApiParameterDescription>> parametersByContainer = context
            .Description.ParameterDescriptions.Where(p =>
                p.Source == BindingSource.Query || p.Source == BindingSource.Path
            )
            .Where(p => p.ModelMetadata.ContainerType is not null)
            .GroupBy(p => p.ModelMetadata.ContainerType!);

        foreach (IGrouping<Type, ApiParameterDescription> group in parametersByContainer)
        {
            Type containerType = group.Key;

            // Resolve the FluentValidation validator for the container type
            Type validatorGenericType = typeof(IValidator<>).MakeGenericType(containerType);
            var validator = scope.ServiceProvider.GetService(validatorGenericType) as IValidator;

            Dictionary<string, List<(IPropertyValidator Validator, IRuleComponent Options)>>? memberValidators = null;
            if (validator is not null)
            {
                IValidatorDescriptor descriptor = validator.CreateDescriptor();
                memberValidators = descriptor
                    .GetMembersWithValidators()
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
            }

            foreach (ApiParameterDescription parameterDescription in group)
            {
                // Find matching OpenAPI parameter by name
                OpenApiParameter? openApiParameter = operation
                    .Parameters.OfType<OpenApiParameter>()
                    .FirstOrDefault(p =>
                        string.Equals(p.Name, parameterDescription.Name, StringComparison.OrdinalIgnoreCase)
                    );

                if (openApiParameter?.Schema is not OpenApiSchema parameterSchema)
                    continue;

                // Ensure parameter schema has a type — ASP.NET Core omits it for
                // value types bound from [FromQuery] complex types
                EnsureSchemaType(parameterSchema, parameterDescription.ModelMetadata.ModelType);

                // Get the property name on the container type (PascalCase)
                string propertyName = parameterDescription.ModelMetadata.PropertyName ?? parameterDescription.Name;

                // Apply FluentValidation rules if available
                if (
                    memberValidators is null
                    || !memberValidators.TryGetValue(
                        propertyName,
                        out List<(IPropertyValidator Validator, IRuleComponent Options)>? rules
                    )
                )
                    continue;

                foreach ((IPropertyValidator propertyValidator, IRuleComponent _) in rules)
                {
                    FluentValidationRuleMapper.ApplyRule(
                        parameterSchema,
                        propertyValidator,
                        markRequired: () => openApiParameter.Required = true
                    );
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Ensures the parameter schema has a single <c>type</c> suitable for OpenAPI 3.0.
    /// .NET 10's schema generator sets multi-type flags (e.g., <c>Integer | String</c>) for numeric
    /// types due to JSON Schema 2020-12 semantics. OpenAPI 3.0 only supports a single type, so the
    /// serializer drops the field entirely when multiple non-null types are present.
    /// This method strips the <c>String</c> flag for numeric types and sets the correct single type.
    /// </summary>
    private static void EnsureSchemaType(OpenApiSchema schema, Type? modelType)
    {
        if (modelType is null)
            return;

        Type type = Nullable.GetUnderlyingType(modelType) ?? modelType;
        bool isNullable = Nullable.GetUnderlyingType(modelType) is not null;

        JsonSchemaType? targetType = type switch
        {
            _ when type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) =>
                JsonSchemaType.Integer,
            _ when type == typeof(decimal) || type == typeof(double) || type == typeof(float) => JsonSchemaType.Number,
            _ when type == typeof(bool) => JsonSchemaType.Boolean,
            _ when type == typeof(string)
                    || type == typeof(Guid)
                    || type == typeof(DateTime)
                    || type == typeof(DateTimeOffset) => JsonSchemaType.String,
            _ => null,
        };

        if (targetType is null)
            return;

        // Check if the schema already has the correct single type
        JsonSchemaType expected = isNullable ? targetType.Value | JsonSchemaType.Null : targetType.Value;
        if (schema.Type == expected)
            return;

        schema.Type = expected;
    }
}
