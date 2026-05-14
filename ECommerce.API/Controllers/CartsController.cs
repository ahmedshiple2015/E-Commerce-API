using ECommerce.API.Contracts;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartsController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public CartsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart([FromQuery] int? userId, [FromQuery] string? sessionId)
    {
        if (!CanAccessCart(userId, sessionId))
        {
            return OwnershipForbidden();
        }

        var cart = await FindCart(userId, sessionId).AsNoTracking().FirstOrDefaultAsync();
        return cart is null ? NotFound() : cart.ToDto();
    }

    [HttpPost("items")]
    [AllowAnonymous]
    public async Task<ActionResult<CartDto>> AddItem([FromQuery] int? userId, [FromQuery] string? sessionId, CartItemRequest request)
    {
        if (!CanAccessCart(userId, sessionId))
        {
            return OwnershipForbidden();
        }

        if (userId is null && string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest("Provide either a userId or a sessionId.");
        }

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
        if (product is null)
        {
            return NotFound("Product was not found.");
        }

        if (request.Quantity <= 0 || product.Stock < request.Quantity)
        {
            return BadRequest("Requested quantity is not available.");
        }

        var cart = await FindCart(userId, sessionId).FirstOrDefaultAsync();
        if (cart is null)
        {
            cart = new Cart { UserId = userId, SessionId = sessionId };
            _db.Carts.Add(cart);
        }

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (item is null)
        {
            cart.Items.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity });
        }
        else
        {
            if (product.Stock < item.Quantity + request.Quantity)
            {
                return BadRequest("Requested quantity is not available.");
            }

            item.Quantity += request.Quantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetCart(userId, sessionId);
    }

    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> UpdateQuantity(int productId, [FromQuery] int? userId, [FromQuery] string? sessionId, CartItemRequest request)
    {
        if (!CanAccessCart(userId, sessionId))
        {
            return OwnershipForbidden();
        }

        var cart = await FindCart(userId, sessionId).FirstOrDefaultAsync();
        var item = cart?.Items.FirstOrDefault(i => i.ProductId == productId);
        if (cart is null || item is null)
        {
            return NotFound();
        }

        var product = await _db.Products.FirstAsync(p => p.Id == productId);
        if (request.Quantity <= 0 || product.Stock < request.Quantity)
        {
            return BadRequest("Requested quantity is not available.");
        }

        item.Quantity = request.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId, [FromQuery] int? userId, [FromQuery] string? sessionId)
    {
        if (!CanAccessCart(userId, sessionId))
        {
            return OwnershipForbidden();
        }

        var cart = await FindCart(userId, sessionId).FirstOrDefaultAsync();
        var item = cart?.Items.FirstOrDefault(i => i.ProductId == productId);
        if (cart is null || item is null)
        {
            return NotFound();
        }

        _db.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<Cart> FindCart(int? userId, string? sessionId)
    {
        var carts = _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).AsQueryable();
        return userId.HasValue
            ? carts.Where(c => c.UserId == userId.Value)
            : carts.Where(c => c.SessionId == sessionId);
    }

    private bool CanAccessCart(int? userId, string? sessionId)
    {
        if (userId.HasValue)
        {
            return CanAccessUser(userId.Value);
        }

        return !string.IsNullOrWhiteSpace(sessionId);
    }
}
