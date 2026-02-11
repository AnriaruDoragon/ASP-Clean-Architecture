using Application.Common.Messaging;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Files.Commands.UploadFile;

/// <summary>
/// Command to upload a file.
/// </summary>
public sealed record UploadFileCommand(IFormFile File) : ICommand<UploadFileResponse>;

/// <summary>
/// Response returned after a successful file upload.
/// </summary>
public sealed record UploadFileResponse(string FileName, long Size, string ContentType);
