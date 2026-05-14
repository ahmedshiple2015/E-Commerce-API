using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace ECommerce.Infrastructure.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IResend _resend;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IResend resend, IConfiguration configuration, ILogger<EmailNotificationService> logger)
    {
        _resend = resend;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(string recipientEmail, string confirmationUrl, CancellationToken cancellationToken = default)
    {
        var message = CreateMessage(
            recipientEmail,
            "Confirm your email address",
            $"""
            <p>Welcome to E-Commerce.</p>
            <p>Please confirm your email address by clicking the link below:</p>
            <p><a href="{confirmationUrl}">Confirm email</a></p>
            """);

        await _resend.EmailSendAsync(message, cancellationToken);
        _logger.LogInformation("Email confirmation message sent to {RecipientEmail}.", recipientEmail);
    }

    public async Task SendOrderStatusChangedAsync(string recipientEmail, int orderId, string status, CancellationToken cancellationToken = default)
    {
        var message = CreateMessage(
            recipientEmail,
            $"Order #{orderId} status update",
            $"""
            <p>Your order <strong>#{orderId}</strong> status changed to <strong>{status}</strong>.</p>
            """);

        await _resend.EmailSendAsync(message, cancellationToken);
        _logger.LogInformation("Order {OrderId} status notification sent to {RecipientEmail}.", orderId, recipientEmail);
    }

    private EmailMessage CreateMessage(string recipientEmail, string subject, string html)
    {
        var from = _configuration["Resend:FromEmail"];
        if (string.IsNullOrWhiteSpace(from))
        {
            throw new InvalidOperationException("Resend:FromEmail is not configured.");
        }

        var message = new EmailMessage
        {
            From = from,
            Subject = subject,
            HtmlBody = html
        };

        message.To.Add(recipientEmail);
        return message;
    }
}
