namespace ECommerce.Application.Payments;

public record CheckoutSessionResult(
    string SessionId,
    string ClientSecret,
    long Amount,
    string Currency,
    string PublishableKey);

public record CheckoutSessionStatusResult(
    string SessionId,
    string Status,
    string PaymentStatus,
    string? PaymentIntentId,
    int? OrderId);
