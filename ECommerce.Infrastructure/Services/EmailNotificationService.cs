using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendOrderStatusChangedAsync(string recipientEmail, int orderId, string status, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Order {OrderId} status changed to {Status}. Notification queued for {RecipientEmail}.", orderId, status, recipientEmail);
        return Task.CompletedTask;
    }
}
