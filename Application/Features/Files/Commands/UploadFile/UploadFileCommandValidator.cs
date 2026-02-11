using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Files.Commands.UploadFile;

/// <summary>
/// Validator for UploadFileCommand demonstrating file validation extensions.
/// </summary>
public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required.")
            .MaxFileSize(10_000_000)
            .WithMessage("File size must not exceed 10 MB.")
            .AllowedContentTypes("image/jpeg", "image/png", "image/webp", "video/mp4")
            .WithMessage("Only JPEG, PNG, WebP, and MP4 files are allowed.")
            .AllowedExtensions(".jpg", ".jpeg", ".png", ".webp", ".mp4")
            .WithMessage("File extension is not allowed.");
    }
}
