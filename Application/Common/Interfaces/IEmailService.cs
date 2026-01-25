namespace Application.Common.Interfaces;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email using a template.
    /// </summary>
    public Task SendTemplateAsync(string templateName, EmailMessage message, object model, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an email message.
/// </summary>
public sealed record EmailMessage
{
    public required string To { get; init; }
    public string? Cc { get; init; }
    public string? Bcc { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsHtml { get; init; } = true;
}
