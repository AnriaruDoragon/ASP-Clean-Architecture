using System.Globalization;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Common.ApiVersioning.OpenApi;

/// <summary>
/// OpenAPI schema transformer that extracts FluentValidation rules and applies them to the schema.
/// Automatically adds constraints like maxLength, minimum, maximum, required, pattern, etc.
/// </summary>
/// <remarks>
/// <para>
/// This transformer inspects registered FluentValidation validators and extracts their rules
/// to enhance OpenAPI schema definitions. This provides clients with validation constraints
/// directly in the API documentation.
/// </para>
/// <para>
/// Supported validation rules:
/// </para>
/// <list type="bullet">
///   <item><description>NotEmpty/NotNull → marks property as required</description></item>
///   <item><description>MaximumLength → sets maxLength</description></item>
///   <item><description>MinimumLength → sets minLength</description></item>
///   <item><description>Length → sets both minLength and maxLength</description></item>
///   <item><description>GreaterThan/GreaterThanOrEqual → sets minimum</description></item>
///   <item><description>LessThan/LessThanOrEqual → sets maximum</description></item>
///   <item><description>InclusiveBetween/ExclusiveBetween → sets minimum and maximum</description></item>
///   <item><description>EmailAddress → sets format to "email"</description></item>
///   <item><description>Matches (regex) → sets pattern</description></item>
/// </list>
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

        IValidatorDescriptor? descriptor = validator.CreateDescriptor();

        foreach (IGrouping<string, (IPropertyValidator Validator, IRuleComponent Options)> member in descriptor.GetMembersWithValidators())
        {
            string propertyName = ToCamelCase(member.Key);

            if (schema.Properties is null || !schema.Properties.TryGetValue(propertyName, out IOpenApiSchema? propertySchema))
                continue;

            if (propertySchema is not OpenApiSchema concreteSchema)
                continue;

            foreach ((IPropertyValidator propertyValidator, IRuleComponent _) in member)
            {
                ApplyValidatorToSchema(concreteSchema, propertyValidator, schema, propertyName);
            }
        }

        return Task.CompletedTask;
    }

    private static void ApplyValidatorToSchema(
        OpenApiSchema propertySchema,
        IPropertyValidator propertyValidator,
        OpenApiSchema parentSchema,
        string propertyName)
    {
        switch (propertyValidator)
        {
            // NotEmpty / NotNull - mark as required
            case INotNullValidator or INotEmptyValidator:
                parentSchema.Required?.Add(propertyName);
                break;

            // MaximumLength
            case IMaximumLengthValidator maxLengthValidator:
                propertySchema.MaxLength = maxLengthValidator.Max;
                break;

            // MinimumLength
            case IMinimumLengthValidator minLengthValidator:
                propertySchema.MinLength = minLengthValidator.Min;
                break;

            // Length (both min and max)
            case ILengthValidator lengthValidator:
                propertySchema.MinLength = lengthValidator.Min;
                propertySchema.MaxLength = lengthValidator.Max;
                break;

            // GreaterThan / GreaterThanOrEqual / LessThan / LessThanOrEqual
            case IComparisonValidator { ValueToCompare: IConvertible convertible } comparisonValidator:
            {
                string value = Convert.ToDecimal(convertible).ToString(CultureInfo.InvariantCulture);

                switch (comparisonValidator.Comparison)
                {
                    case Comparison.GreaterThan:
                        propertySchema.Minimum = value;
                        propertySchema.ExclusiveMinimum = "true";
                        break;
                    case Comparison.GreaterThanOrEqual:
                        propertySchema.Minimum = value;
                        break;
                    case Comparison.LessThan:
                        propertySchema.Maximum = value;
                        propertySchema.ExclusiveMaximum = "true";
                        break;
                    case Comparison.LessThanOrEqual:
                        propertySchema.Maximum = value;
                        break;
                }

                break;
            }

            // InclusiveBetween / ExclusiveBetween
            case IBetweenValidator betweenValidator:
            {
                if (betweenValidator.From is IConvertible fromConvertible)
                    propertySchema.Minimum = Convert.ToDecimal(fromConvertible).ToString(CultureInfo.InvariantCulture);

                if (betweenValidator.To is IConvertible toConvertible)
                    propertySchema.Maximum = Convert.ToDecimal(toConvertible).ToString(CultureInfo.InvariantCulture);

                break;
            }

            // Email
            case not null when propertyValidator.GetType().Name.Contains("EmailValidator"):
                propertySchema.Format = "email";
                break;

            // Regular expression
            case IRegularExpressionValidator regexValidator:
                propertySchema.Pattern = regexValidator.Expression;
                break;
        }
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
            return name;

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
