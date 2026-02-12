using Application.Common.Models;
using Application.Features.Files.Commands.UploadFile;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Web.API.Extensions;
using Web.API.Models;

namespace Web.API.Controllers.V1;

/// <summary>
/// Example controller demonstrating file upload with validation.
/// </summary>
[ApiController]
[Route("[controller]")]
[EnableRateLimiting("endpoint")]
[RateLimit("Api", Per.User)]
public class FilesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Uploads a file (image or video).
    /// </summary>
    [HttpPost("upload")]
    [EndpointSummary("Upload file")]
    [ProducesResponseType<UploadFileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        var command = new UploadFileCommand(file);
        Result<UploadFileResponse> result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }
}
