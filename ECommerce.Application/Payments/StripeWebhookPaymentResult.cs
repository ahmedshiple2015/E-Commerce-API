using ECommerce.Domain.Enums;

namespace ECommerce.Application.Payments;

public record StripeWebhookPaymentResult(
    string EventId,
    string PaymentIntentId,
    int? OrderId,
    decimal Amount,
    PaymentStatus PaymentStatus);
