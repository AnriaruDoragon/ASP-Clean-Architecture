using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Common.ApiVersioning.OpenApi;

/// <summary>
/// OpenAPI schema transformer that extracts FluentValidation rules and applies them
/// to request body schemas. Works with JSON body types (commands, request DTOs).
/// </summary>
/// <remarks>
/// For query/route parameter validation, see <see cref="FluentValidationOperationTransformer"/>.
/// Both transformers share rule-mapping logic via <see cref="FluentValidationRuleMapper"/>.
/// </remarks>
public sealed class FluentValidationSchemaTransformer(IServiceProvider serviceProvider) : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        Type validatorType = typeof(IValidator<>).MakeGenericType(context.JsonTypeInfo.Type);

        using IServiceScope scope = serviceProvider.CreateScope();

        if (scope.ServiceProvider.GetService(validatorType) is not IValidator validator)
            return Task.CompletedTask;

        IValidatorDescriptor descriptor = validator.CreateDescriptor();

        foreach (IGrouping<string, (IPropertyValidator Validator, IRuleComponent Options)> member in descriptor.GetMembersWithValidators())
        {
            string propertyName = FluentValidationRuleMapper.ToCamelCase(member.Key);

            if (schema.Properties is null || !schema.Properties.TryGetValue(propertyName, out IOpenApiSchema? propertySchema))
                continue;

            OpenApiSchema? concreteSchema = propertySchema switch
            {
                OpenApiSchema s => s,
                OpenApiSchemaReference { Target: OpenApiSchema target } => target,
                _ => null
            };

            if (concreteSchema is null)
                continue;

            foreach ((IPropertyValidator propertyValidator, IRuleComponent _) in member)
            {
                FluentValidationRuleMapper.ApplyRule(
                    concreteSchema,
                    propertyValidator,
                    markRequired: () => schema.Required?.Add(propertyName));
            }
        }

        return Task.CompletedTask;
    }
}
