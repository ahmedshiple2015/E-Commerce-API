namespace ECommerce.Application.Interfaces;

public interface IEmailNotificationService
{
    Task SendEmailConfirmationAsync(string recipientEmail, string confirmationUrl, CancellationToken cancellationToken = default);
    Task SendOrderStatusChangedAsync(string recipientEmail, int orderId, string status, CancellationToken cancellationToken = default);
}
