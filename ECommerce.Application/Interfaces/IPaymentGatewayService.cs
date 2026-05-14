using ECommerce.Domain.Enums;
using ECommerce.Application.Payments;

namespace ECommerce.Application.Interfaces;

public interface IPaymentGatewayService
{
    bool Supports(PaymentMethod paymentMethod);
    Task<bool> VerifyWebhookAsync(PaymentMethod paymentMethod, string payload, string? signature, CancellationToken cancellationToken = default);
    Task<PaymentIntentResult> CreatePaymentIntentAsync(int orderId, decimal amount, string currency, CancellationToken cancellationToken = default);
    Task<StripeWebhookPaymentResult> VerifyStripeWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);
}
