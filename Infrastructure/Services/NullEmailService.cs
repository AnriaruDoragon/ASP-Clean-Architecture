using Application.Common.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Placeholder email service that throws NotImplementedException.
/// TODO: Implement a real email service (SMTP, SendGrid, etc.)
/// </summary>
public sealed class NullEmailService : IEmailService
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        // TODO: Implement email sending
        throw new NotImplementedException(
            "Email service not configured. Implement IEmailService with your preferred email provider."
        );
    }

    public Task SendTemplateAsync(
        string templateName,
        EmailMessage message,
        object model,
        CancellationToken cancellationToken = default
    )
    {
        // TODO: Implement template-based email sending
        throw new NotImplementedException(
            "Email service not configured. Implement IEmailService with your preferred email provider."
        );
    }
}
