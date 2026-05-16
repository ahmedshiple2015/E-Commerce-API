using ECommerce.Application.Interfaces;
using ECommerce.Application.Payments;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using PaymentMethod = ECommerce.Domain.Enums.PaymentMethod;
using PaymentStatus = ECommerce.Domain.Enums.PaymentStatus;

namespace ECommerce.Infrastructure.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly IConfiguration _configuration;

    public PaymentGatewayService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool Supports(PaymentMethod paymentMethod)
    {
        return paymentMethod is PaymentMethod.CreditCard;
    }

    public Task<bool> VerifyWebhookAsync(PaymentMethod paymentMethod, string payload, string? signature, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Supports(paymentMethod) && !string.IsNullOrWhiteSpace(signature));
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(int orderId, decimal amount, string currency, CancellationToken cancellationToken = default)
    {
        var secretKey = GetRequiredSetting("Stripe:SecretKey");
        var publishableKey = GetRequiredSetting("Stripe:PublishableKey");
        var service = new PaymentIntentService(new StripeClient(secretKey));
        var normalizedCurrency = string.IsNullOrWhiteSpace(currency)
            ? _configuration["Stripe:Currency"] ?? "usd"
            : currency.ToLowerInvariant();

        var options = new PaymentIntentCreateOptions
        {
            Amount = ToMinorUnits(amount),
            Currency = normalizedCurrency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            },
            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = orderId.ToString()
            }
        };

        var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);
        return new PaymentIntentResult(
            paymentIntent.Id,
            paymentIntent.ClientSecret,
            paymentIntent.Amount,
            paymentIntent.Currency,
            publishableKey);
    }

    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(int orderId, decimal amount, string currency, string returnUrl, string? customerEmail = null, CancellationToken cancellationToken = default)
    {
        var secretKey = GetRequiredSetting("Stripe:SecretKey");
        var publishableKey = GetRequiredSetting("Stripe:PublishableKey");
        var normalizedCurrency = string.IsNullOrWhiteSpace(currency)
            ? _configuration["Stripe:Currency"] ?? "usd"
            : currency.ToLowerInvariant();

        var service = new SessionService(new StripeClient(secretKey));
        var options = new SessionCreateOptions
        {
            UiMode = "elements",
            Mode = "payment",
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
                ? "http://localhost:4200/checkout/complete?session_id={CHECKOUT_SESSION_ID}"
                : returnUrl,
            CustomerEmail = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail,
            CustomerCreation = string.IsNullOrWhiteSpace(customerEmail) ? "always" : null,
            PaymentMethodTypes = new List<string> { "card" },
            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = orderId.ToString()
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["orderId"] = orderId.ToString()
                }
            },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = normalizedCurrency,
                        UnitAmount = ToMinorUnits(amount),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Order #{orderId}"
                        }
                    }
                }
            }
        };

        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);
        return new CheckoutSessionResult(
            session.Id,
            session.ClientSecret,
            ToMinorUnits(amount),
            normalizedCurrency,
            publishableKey);
    }

    public async Task<CheckoutSessionStatusResult> GetCheckoutSessionStatusAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var secretKey = GetRequiredSetting("Stripe:SecretKey");
        var service = new SessionService(new StripeClient(secretKey));
        var session = await service.GetAsync(sessionId, cancellationToken: cancellationToken);
        session.Metadata.TryGetValue("orderId", out var orderIdValue);
        var orderId = int.TryParse(orderIdValue, out var parsedOrderId) ? parsedOrderId : (int?)null;
        return new CheckoutSessionStatusResult(session.Id, session.Status, session.PaymentStatus, session.PaymentIntentId, orderId);
    }

    public Task<StripeWebhookPaymentResult> VerifyStripeWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        var webhookSecret = GetRequiredSetting("Stripe:WebhookSecret");
        var stripeEvent = EventUtility.ConstructEvent(payload, signature, webhookSecret);

        if (stripeEvent.Data.Object is Session session)
        {
            session.Metadata.TryGetValue("orderId", out var sessionOrderIdValue);
            var sessionOrderId = int.TryParse(sessionOrderIdValue, out var parsedSessionOrderId) ? parsedSessionOrderId : (int?)null;
            var sessionStatus = stripeEvent.Type == EventTypes.CheckoutSessionCompleted || session.PaymentStatus == "paid"
                ? PaymentStatus.Completed
                : PaymentStatus.Failed;

            return Task.FromResult(new StripeWebhookPaymentResult(
                stripeEvent.Id,
                session.PaymentIntentId ?? session.Id,
                sessionOrderId,
                0,
                sessionStatus));
        }

        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            throw new InvalidOperationException($"Unsupported Stripe event object: {stripeEvent.Data.Object.Object}.");
        }

        paymentIntent.Metadata.TryGetValue("orderId", out var orderIdValue);
        var orderId = int.TryParse(orderIdValue, out var parsedOrderId) ? parsedOrderId : (int?)null;
        var status = stripeEvent.Type == EventTypes.PaymentIntentSucceeded
            ? PaymentStatus.Completed
            : PaymentStatus.Failed;

        return Task.FromResult(new StripeWebhookPaymentResult(
            stripeEvent.Id,
            paymentIntent.Id,
            orderId,
            paymentIntent.AmountReceived / 100m,
            status));
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

    private static long ToMinorUnits(decimal amount)
    {
        return decimal.ToInt64(decimal.Round(amount * 100, 0, MidpointRounding.AwayFromZero));
    }
}
