using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Validators;

/// <summary>
/// Fluent validation extension methods for <see cref="IFormFile"/> properties.
/// These validators are introspectable by OpenAPI transformers to display file constraints in Scalar UI.
/// </summary>
/// <example>
/// <code>
/// RuleFor(x => x.Image)
///     .NotNull()
///     .MaxFileSize(5_000_000)
///     .AllowedContentTypes("image/jpeg", "image/png")
///     .AllowedExtensions(".jpg", ".jpeg", ".png");
/// </code>
/// </example>
public static class FileValidationExtensions
{
    /// <summary>
    /// Validates that the file size does not exceed the specified maximum.
    /// </summary>
    /// <param name="ruleBuilder">The rule builder for the IFormFile property.</param>
    /// <param name="maxSizeInBytes">Maximum file size in bytes.</param>
    public static IRuleBuilderOptions<T, IFormFile> MaxFileSize<T>(
        this IRuleBuilder<T, IFormFile> ruleBuilder, long maxSizeInBytes) =>
        ruleBuilder.SetValidator(new MaxFileSizeValidator<T>(maxSizeInBytes));

    /// <summary>
    /// Validates that the file's MIME content type is in the allowed list.
    /// </summary>
    /// <param name="ruleBuilder">The rule builder for the IFormFile property.</param>
    /// <param name="contentTypes">Allowed MIME types (e.g., "image/jpeg", "image/png").</param>
    public static IRuleBuilderOptions<T, IFormFile> AllowedContentTypes<T>(
        this IRuleBuilder<T, IFormFile> ruleBuilder, params string[] contentTypes) =>
        ruleBuilder.SetValidator(new AllowedContentTypesValidator<T>(contentTypes));

    /// <summary>
    /// Validates that the file extension is in the allowed list.
    /// </summary>
    /// <param name="ruleBuilder">The rule builder for the IFormFile property.</param>
    /// <param name="extensions">Allowed extensions (e.g., ".jpg", ".png"). Leading dot is optional.</param>
    public static IRuleBuilderOptions<T, IFormFile> AllowedExtensions<T>(
        this IRuleBuilder<T, IFormFile> ruleBuilder, params string[] extensions) =>
        ruleBuilder.SetValidator(new AllowedExtensionsValidator<T>(extensions));
}
