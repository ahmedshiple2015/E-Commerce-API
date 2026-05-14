namespace ECommerce.Application.Interfaces;

public interface IEmailNotificationService
{
    Task SendOrderStatusChangedAsync(string recipientEmail, int orderId, string status, CancellationToken cancellationToken = default);
}
