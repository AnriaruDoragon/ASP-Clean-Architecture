using Application.Common.Messaging;
using Application.Common.Models;

namespace Application.Features.Files.Commands.UploadFile;

/// <summary>
/// Example handler that returns file metadata.
/// Replace with actual storage logic (local disk, S3, Azure Blob, etc.).
/// </summary>
public sealed class UploadFileCommandHandler : ICommandHandler<UploadFileCommand, UploadFileResponse>
{
    public Task<Result<UploadFileResponse>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        // TODO: Implement actual file storage (local disk, S3, Azure Blob, etc.)
        var response = new UploadFileResponse(request.File.FileName, request.File.Length, request.File.ContentType);

        return Task.FromResult(Result.Success(response));
    }
}
