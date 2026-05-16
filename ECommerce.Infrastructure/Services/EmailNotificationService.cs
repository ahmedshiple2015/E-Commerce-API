using System.Net.Http.Headers;
using System.Net.Http.Json;
using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IConfiguration configuration, ILogger<EmailNotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(string recipientEmail, string confirmationUrl, CancellationToken cancellationToken = default)
    {
        await SendAsync(
            recipientEmail,
            "Confirm your email address",
            $"""
            <p>Welcome to Nexora Gear.</p>
            <p>Please confirm your email address by clicking the link below:</p>
            <p><a href="{confirmationUrl}">Confirm email</a></p>
            """,
            cancellationToken);

        _logger.LogInformation("Email confirmation message sent to {RecipientEmail} through Mailtrap API.", recipientEmail);
    }

    public async Task SendOrderStatusChangedAsync(string recipientEmail, int orderId, string status, CancellationToken cancellationToken = default)
    {
        await SendAsync(
            recipientEmail,
            $"Order #{orderId} status update",
            $"""
            <p>Your order <strong>#{orderId}</strong> status changed to <strong>{status}</strong>.</p>
            <p>Thanks for shopping with Nexora Gear.</p>
            """,
            cancellationToken);

        _logger.LogInformation("Order {OrderId} status notification sent to {RecipientEmail} through Mailtrap API.", orderId, recipientEmail);
    }

    private async Task SendAsync(string recipientEmail, string subject, string html, CancellationToken cancellationToken)
    {
        var apiToken = GetRequiredSetting("Mailtrap:ApiToken");
        var apiUrl = _configuration["Mailtrap:ApiUrl"] ?? "https://send.api.mailtrap.io/api/send";
        var fromEmail = GetRequiredSetting("Mailtrap:FromEmail");
        var fromName = _configuration["Mailtrap:FromName"] ?? "Nexora Gear";
        var overrideRecipientEmail = _configuration["Mailtrap:OverrideRecipientEmail"];
        var actualRecipientEmail = string.IsNullOrWhiteSpace(overrideRecipientEmail)
            ? recipientEmail
            : overrideRecipientEmail;
        var finalHtml = string.IsNullOrWhiteSpace(overrideRecipientEmail)
            ? html
            : $"""
              <p><strong>Development email override.</strong></p>
              <p>Original recipient: {recipientEmail}</p>
              <hr />
              {html}
              """;

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var payload = new
        {
            from = new { email = fromEmail, name = fromName },
            to = new[] { new { email = actualRecipientEmail } },
            subject,
            html = finalHtml
        };

        var response = await client.PostAsJsonAsync(apiUrl, payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Mailtrap API send failed with {(int)response.StatusCode}: {body}");
        }
    }

    private string GetRequiredSetting(string key)
    {
        var value = _configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{key} is not configured.");
        }

        return value;
    }
}
