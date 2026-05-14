namespace ECommerce.Application.Payments;

public record PaymentIntentResult(
    string PaymentIntentId,
    string ClientSecret,
    long Amount,
    string Currency,
    string PublishableKey);
