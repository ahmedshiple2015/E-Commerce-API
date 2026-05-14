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
public class OrdersController : ApiControllerBase
{
    private const decimal TaxRate = 0.14m;
    private const decimal ShippingRate = 50m;
    private readonly AppDbContext _db;
    private readonly IEmailNotificationService _emailNotificationService;

    public OrdersController(AppDbContext db, IEmailNotificationService emailNotificationService)
    {
        _db = db;
        _emailNotificationService = emailNotificationService;
    }

    [HttpGet]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] int? userId)
    {
        if (!IsAdmin)
        {
            if (CurrentUserId is null)
            {
                return Unauthorized();
            }

            userId = CurrentUserId;
        }

        var orders = _db.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .Include(o => o.Payment)
            .Include(o => o.StatusHistory)
            .AsQueryable();

        if (userId.HasValue)
        {
            orders = orders.Where(o => o.UserId == userId.Value);
        }

        var result = await orders.AsNoTracking().ToListAsync();
        return result.Select(o => o.ToDto()).ToList();
    }

    [HttpPost("checkout")]
    [AllowAnonymous]
    public async Task<ActionResult<OrderDto>> Checkout(CheckoutRequest request)
    {
        if (request.UserId.HasValue && !CanAccessUser(request.UserId.Value))
        {
            return OwnershipForbidden();
        }

        if (request.UserId is null && string.IsNullOrWhiteSpace(request.SessionId))
        {
            return BadRequest("Provide either a userId or a sessionId.");
        }

        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => request.UserId.HasValue ? c.UserId == request.UserId.Value : c.SessionId == request.SessionId);

        if (cart is null || cart.Items.Count == 0)
        {
            return BadRequest("Cart is empty.");
        }

        foreach (var item in cart.Items)
        {
            if (item.Product.Stock < item.Quantity)
            {
                return BadRequest($"{item.Product.Name} does not have enough stock.");
            }
        }

        var subtotal = cart.Items.Sum(i => i.Product.Price * i.Quantity);
        var tax = Math.Round(subtotal * TaxRate, 2);
        var order = new Order
        {
            UserId = request.UserId,
            ShippingAddress = request.ShippingAddress,
            TaxAmount = tax,
            ShippingAmount = ShippingRate,
            TotalAmount = subtotal + tax + ShippingRate,
            Status = OrderStatus.Pending,
            OrderItems = cart.Items.Select(i => new OrderItem { ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.Product.Price }).ToList(),
            Payment = new Payment { PaymentMethod = request.PaymentMethod, PaymentStatus = PaymentStatus.Pending, Amount = subtotal + tax + ShippingRate },
            StatusHistory = new List<OrderStatusHistory> { new() { Status = OrderStatus.Pending, Notes = "Order placed." } }
        };

        foreach (var item in cart.Items)
        {
            item.Product.Stock -= item.Quantity;
        }

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order.ToDto());
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatusRequest request)
    {
        var order = await _db.Orders
            .Include(o => o.StatusHistory)
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        if (IsSeller && !IsAdmin)
        {
            var sellerId = await _db.Sellers
                .Where(s => s.UserId == CurrentUserId)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync();

            if (sellerId is null || !order.OrderItems.Any(i => i.Product.SellerId == sellerId.Value))
            {
                return Forbid();
            }
        }

        order.Status = request.Status;
        order.StatusHistory.Add(new OrderStatusHistory { Status = request.Status, Notes = request.Notes });
        await _db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(order.User?.Email))
        {
            await _emailNotificationService.SendOrderStatusChangedAsync(order.User.Email, order.Id, request.Status.ToString());
        }

        return NoContent();
    }
}
