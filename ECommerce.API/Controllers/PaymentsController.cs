using ECommerce.API.Contracts;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPaymentGatewayService _paymentGatewayService;

    public PaymentsController(AppDbContext db, IPaymentGatewayService paymentGatewayService)
    {
        _db = db;
        _paymentGatewayService = paymentGatewayService;
    }

    [HttpPost("stripe/payment-intents")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<CreatePaymentIntentResponse>> CreateStripePaymentIntent(CreatePaymentIntentRequest request, CancellationToken cancellationToken)
    {
        var payment = await _db.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId, cancellationToken);

        if (payment is null)
        {
            return NotFound("Payment was not found for this order.");
        }

        if (payment.Order.UserId is int userId && !CanAccessUser(userId))
        {
            return OwnershipForbidden();
        }

        if (payment.Order.UserId is null && !IsAdmin)
        {
            return Forbid();
        }

        if (payment.PaymentMethod != PaymentMethod.CreditCard)
        {
            return BadRequest("Stripe PaymentIntents are available only for credit card payments.");
        }

        var result = await _paymentGatewayService.CreatePaymentIntentAsync(
            payment.OrderId,
            payment.Amount,
            request.Currency ?? "usd",
            cancellationToken);

        payment.GatewayTransactionId = result.PaymentIntentId;
        payment.PaymentStatus = PaymentStatus.Pending;
        await _db.SaveChangesAsync(cancellationToken);

        return new CreatePaymentIntentResponse(
            result.PaymentIntentId,
            result.ClientSecret,
            result.Amount,
            result.Currency,
            result.PublishableKey);
    }

    [HttpPost("stripe/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveStripeWebhook(CancellationToken cancellationToken)
    {
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature))
        {
            return Unauthorized("Stripe signature is missing.");
        }

        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var result = await _paymentGatewayService.VerifyStripeWebhookAsync(payload, signature, cancellationToken);

        if (result.OrderId is null)
        {
            return BadRequest("Stripe webhook is missing the orderId metadata.");
        }

        var payment = await _db.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.StatusHistory)
            .FirstOrDefaultAsync(p => p.OrderId == result.OrderId.Value, cancellationToken);

        if (payment is null)
        {
            return NotFound();
        }

        payment.GatewayTransactionId = result.PaymentIntentId;
        payment.Amount = result.Amount > 0 ? result.Amount : payment.Amount;
        payment.PaymentStatus = result.PaymentStatus;

        if (payment.PaymentStatus == PaymentStatus.Completed)
        {
            payment.Order.Status = OrderStatus.Processing;
            payment.Order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = OrderStatus.Processing,
                Notes = $"Stripe payment confirmed by webhook event {result.EventId}."
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveWebhook(PaymentWebhookRequest request, [FromHeader(Name = "X-Payment-Signature")] string? signature)
    {
        var verified = await _paymentGatewayService.VerifyWebhookAsync(
            request.PaymentMethod,
            $"{request.OrderId}:{request.GatewayTransactionId}:{request.Status}:{request.Amount}",
            signature);

        if (!verified)
        {
            return Unauthorized("Payment webhook signature is invalid.");
        }

        var payment = await _db.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.StatusHistory)
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId);
        if (payment is null)
        {
            return NotFound();
        }

        payment.GatewayTransactionId = request.GatewayTransactionId;
        payment.Amount = request.Amount;
        payment.PaymentStatus = string.Equals(request.Status, "completed", StringComparison.OrdinalIgnoreCase)
            ? PaymentStatus.Completed
            : PaymentStatus.Failed;

        if (payment.PaymentStatus == PaymentStatus.Completed)
        {
            payment.Order.Status = OrderStatus.Processing;
            payment.Order.StatusHistory.Add(new Domain.Entities.OrderStatusHistory
            {
                Status = OrderStatus.Processing,
                Notes = "Payment confirmed by gateway webhook."
            });
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("orders/{orderId:int}")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> GetPayment(int orderId)
    {
        var payment = await _db.Payments
            .Include(p => p.Order)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
        if (payment?.Order.UserId is int userId && !CanAccessUser(userId))
        {
            return OwnershipForbidden();
        }

        if (payment?.Order.UserId is null && !IsAdmin)
        {
            return Forbid();
        }

        return payment is null ? NotFound() : Ok(payment.ToDto());
    }
}
