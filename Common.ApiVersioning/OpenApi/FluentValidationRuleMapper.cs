using System.Globalization;
using System.Reflection;
using FluentValidation.Validators;
using Microsoft.OpenApi;

namespace Common.ApiVersioning.OpenApi;

/// <summary>
/// Shared utility for mapping FluentValidation rules to OpenAPI schema constraints.
/// Used by both <see cref="FluentValidationSchemaTransformer"/> (body schemas) and
/// <see cref="FluentValidationOperationTransformer"/> (query/route parameters).
/// </summary>
public static class FluentValidationRuleMapper
{
    /// <summary>
    /// Well-known regex patterns mapped to human-readable descriptions.
    /// </summary>
    private static readonly Dictionary<string, string> s_wellKnownPatterns = new(StringComparer.Ordinal)
    {
        ["[A-Z]"] = "Must contain at least one uppercase letter",
        ["[a-z]"] = "Must contain at least one lowercase letter",
        ["[0-9]"] = "Must contain at least one digit",
        [@"\d"] = "Must contain at least one digit",
        ["[^a-zA-Z0-9]"] = "Must contain at least one special character",
        [@"[\W]"] = "Must contain at least one special character",
        [@"^\S+$"] = "Must not contain whitespace",
        [@"^\S*$"] = "Must not contain whitespace",
        [@"^[^\s]+$"] = "Must not contain whitespace",
        [@"^\d+$"] = "Must contain only digits",
        [@"^[a-zA-Z]+$"] = "Must contain only letters",
        [@"^[a-zA-Z0-9]+$"] = "Must contain only letters and digits",
    };

    /// <summary>
    /// Applies a single FluentValidation property validator to an OpenAPI schema.
    /// </summary>
    /// <param name="propertySchema">The property schema to modify.</param>
    /// <param name="propertyValidator">The FluentValidation property validator.</param>
    /// <param name="markRequired">Action to mark the property as required (differs for body schemas vs parameters).</param>
    public static void ApplyRule(
        OpenApiSchema propertySchema,
        IPropertyValidator propertyValidator,
        Action? markRequired = null
    )
    {
        switch (propertyValidator)
        {
            // NotEmpty / NotNull — mark as required
            case INotNullValidator or INotEmptyValidator:
                markRequired?.Invoke();
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
                ApplyComparisonRule(propertySchema, comparisonValidator, convertible);
                break;

            // InclusiveBetween / ExclusiveBetween
            case IBetweenValidator betweenValidator:
                ApplyBetweenRule(propertySchema, betweenValidator);
                break;

            // Email
            case not null when propertyValidator.GetType().Name.Contains("EmailValidator"):
                propertySchema.Format = "email";
                break;

            // Credit card
            case not null when propertyValidator.GetType().Name.Contains("CreditCardValidator"):
                AppendDescription(propertySchema, "Must be a valid credit card number");
                break;

            // Regular expression — detect well-known patterns for human-readable descriptions
            case IRegularExpressionValidator regexValidator:
                ApplyRegexRule(propertySchema, regexValidator.Expression);
                break;

            // Enum validator
            case not null when TryGetEnumDescription(propertyValidator, out string? enumDesc) && enumDesc is not null:
                AppendDescription(propertySchema, enumDesc);
                break;

            // Equal / NotEqual
            case not null
                when propertyValidator.GetType().Name.Contains("EqualValidator")
                    && TryGetPropertyValue<object>(propertyValidator, "ValueToCompare", out var eqVal):
                AppendDescription(
                    propertySchema,
                    propertyValidator.GetType().Name.Contains("NotEqual")
                        ? $"Must not equal: {eqVal}"
                        : $"Must equal: {eqVal}"
                );
                break;

            // ScalePrecision
            case not null
                when TryGetPropertyValue(propertyValidator, "Scale", out int scale)
                    && TryGetPropertyValue(propertyValidator, "Precision", out int precision):
                AppendDescription(propertySchema, $"Max {scale} decimal places, {precision} digits total");
                break;

            // File validators (detected via reflection to avoid coupling to Application layer)
            case not null when TryGetPropertyValue(propertyValidator, "MaxSizeInBytes", out long maxSize):
                AppendDescription(propertySchema, $"Max file size: {FormatFileSize(maxSize)}");
                break;

            case not null
                when TryGetPropertyValue<IReadOnlyList<string>>(
                    propertyValidator,
                    "ContentTypes",
                    out var contentTypes
                ):
                AppendDescription(propertySchema, $"Allowed types: {string.Join(", ", contentTypes)}");
                break;

            case not null
                when TryGetPropertyValue<IReadOnlyList<string>>(propertyValidator, "Extensions", out var extensions):
                AppendDescription(propertySchema, $"Allowed extensions: {string.Join(", ", extensions)}");
                break;
        }
    }

    /// <summary>
    /// Converts a PascalCase property name to camelCase for JSON/OpenAPI matching.
    /// </summary>
    public static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
            return name;

        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static void ApplyComparisonRule(
        OpenApiSchema schema,
        IComparisonValidator comparisonValidator,
        IConvertible convertible
    )
    {
        string value = Convert.ToDecimal(convertible).ToString(CultureInfo.InvariantCulture);

        switch (comparisonValidator.Comparison)
        {
            case Comparison.GreaterThan:
                schema.Minimum = value;
                schema.ExclusiveMinimum = "true";
                break;
            case Comparison.GreaterThanOrEqual:
                schema.Minimum = value;
                break;
            case Comparison.LessThan:
                schema.Maximum = value;
                schema.ExclusiveMaximum = "true";
                break;
            case Comparison.LessThanOrEqual:
                schema.Maximum = value;
                break;
        }
    }

    private static void ApplyBetweenRule(OpenApiSchema schema, IBetweenValidator betweenValidator)
    {
        if (betweenValidator.From is IConvertible fromConvertible)
            schema.Minimum = Convert.ToDecimal(fromConvertible).ToString(CultureInfo.InvariantCulture);

        if (betweenValidator.To is IConvertible toConvertible)
            schema.Maximum = Convert.ToDecimal(toConvertible).ToString(CultureInfo.InvariantCulture);
    }

    private static void ApplyRegexRule(OpenApiSchema schema, string expression)
    {
        if (s_wellKnownPatterns.TryGetValue(expression, out string? description))
        {
            // Use human-readable description instead of raw regex
            AppendDescription(schema, description);
        }
        else
        {
            // Unknown pattern — set as OpenAPI pattern for client-side validation
            schema.Pattern = expression;
        }
    }

    private static bool TryGetEnumDescription(IPropertyValidator validator, out string? description)
    {
        description = null;
        Type validatorType = validator.GetType();

        if (!validatorType.Name.Contains("EnumValidator"))
            return false;

        // Try to get the enum type from the generic argument
        Type? enumType = validatorType.GetGenericArguments().FirstOrDefault(t => t.IsEnum);
        if (enumType is null)
            return false;

        string[] names = Enum.GetNames(enumType);
        description = $"Allowed values: {string.Join(", ", names)}";
        return true;
    }

    private static void AppendDescription(OpenApiSchema schema, string text)
    {
        if (string.IsNullOrEmpty(schema.Description))
            schema.Description = text;
        else
            schema.Description = $"{schema.Description}. {text}";
    }

    private static bool TryGetPropertyValue<TValue>(object obj, string propertyName, out TValue value)
    {
        value = default!;

        PropertyInfo? prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop is null || !typeof(TValue).IsAssignableFrom(prop.PropertyType))
            return false;

        object? raw = prop.GetValue(obj);
        if (raw is not TValue typed)
            return false;

        value = typed;
        return true;
    }

    private static string FormatFileSize(long bytes) =>
        bytes switch
        {
            >= 1_073_741_824 => $"{bytes / 1_073_741_824.0:F1} GB",
            >= 1_048_576 => $"{bytes / 1_048_576.0:F1} MB",
            >= 1_024 => $"{bytes / 1_024.0:F1} KB",
            _ => $"{bytes} bytes",
        };
}
