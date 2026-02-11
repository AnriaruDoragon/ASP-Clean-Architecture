using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Validators;

/// <summary>
/// Validates that a file does not exceed the specified size in bytes.
/// Introspectable by OpenAPI transformers via the <see cref="MaxSizeInBytes"/> property.
/// </summary>
public sealed class MaxFileSizeValidator<T>(long maxSizeInBytes) : PropertyValidator<T, IFormFile>
{
    /// <summary>
    /// Maximum allowed file size in bytes.
    /// </summary>
    public long MaxSizeInBytes { get; } = maxSizeInBytes;

    public override string Name => "MaxFileSizeValidator";

    public override bool IsValid(ValidationContext<T> context, IFormFile file) => file.Length <= MaxSizeInBytes;

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "File size must not exceed {MaxSize}.";
}

/// <summary>
/// Validates that a file's content type is in the allowed list.
/// Introspectable by OpenAPI transformers via the <see cref="ContentTypes"/> property.
/// </summary>
public sealed class AllowedContentTypesValidator<T>(params string[] contentTypes) : PropertyValidator<T, IFormFile>
{
    /// <summary>
    /// Allowed MIME content types.
    /// </summary>
    public IReadOnlyList<string> ContentTypes { get; } = contentTypes;

    public override string Name => "AllowedContentTypesValidator";

    public override bool IsValid(ValidationContext<T> context, IFormFile file) => ContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase);

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "File type is not allowed. Allowed types: {AllowedTypes}.";
}

/// <summary>
/// Validates that a file's extension is in the allowed list.
/// Introspectable by OpenAPI transformers via the <see cref="Extensions"/> property.
/// </summary>
public sealed class AllowedExtensionsValidator<T>(params string[] extensions) : PropertyValidator<T, IFormFile>
{
    /// <summary>
    /// Allowed file extensions (e.g., ".jpg", ".png").
    /// </summary>
    public IReadOnlyList<string> Extensions { get; } = extensions
        .Select(e => e.StartsWith('.') ? e : $".{e}")
        .ToArray();

    public override string Name => "AllowedExtensionsValidator";

    public override bool IsValid(ValidationContext<T> context, IFormFile file)
    {
        string extension = Path.GetExtension(file.FileName);
        return Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "File extension is not allowed. Allowed extensions: {AllowedExtensions}.";
}
