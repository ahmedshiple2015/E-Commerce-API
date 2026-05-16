using ECommerce.API.Contracts;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SellersController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public SellersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    [Authorize(Roles = "Customer,Seller,Admin")]
    public async Task<ActionResult<SellerDto>> GetCurrentSeller()
    {
        if (CurrentUserId is null)
        {
            return Unauthorized();
        }

        var seller = await _db.Sellers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == CurrentUserId.Value);

        return seller is null ? NotFound() : seller.ToDto();
    }
    [HttpPost]
    [Authorize(Roles = "Customer,Seller,Admin")]
    public async Task<ActionResult<SellerDto>> RegisterSeller(SellerRequest request)
    {
        if (!CanAccessUser(request.UserId))
        {
            return OwnershipForbidden();
        }

        if (!await _db.Users.AnyAsync(u => u.Id == request.UserId))
        {
            return NotFound("User was not found.");
        }

        if (await _db.Sellers.AnyAsync(s => s.UserId == request.UserId))
        {
            return Conflict("User already has a seller profile.");
        }

        var seller = new Seller
        {
            UserId = request.UserId,
            StoreName = request.StoreName,
            BusinessRegistration = request.BusinessRegistration
        };

        _db.Sellers.Add(seller);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSellerProducts), new { id = seller.Id }, seller.ToDto());
    }

    [HttpGet("{id:int}/products")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetSellerProducts(int id)
    {
        if (!IsAdmin && !await _db.Sellers.AnyAsync(s => s.Id == id && s.UserId == CurrentUserId))
        {
            return Forbid();
        }

        var products = await _db.Products
            .Where(p => p.SellerId == id)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .AsNoTracking()
            .ToListAsync();

        return products.Select(p => p.ToDto()).ToList();
    }

    [HttpGet("{id:int}/orders")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetSellerOrders(int id)
    {
        if (!IsAdmin && !await _db.Sellers.AnyAsync(s => s.Id == id && s.UserId == CurrentUserId))
        {
            return Forbid();
        }

        var orders = await _db.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .Include(o => o.Payment)
            .Include(o => o.StatusHistory)
            .Where(o => o.OrderItems.Any(i => i.Product.SellerId == id))
            .AsNoTracking()
            .ToListAsync();

        return orders.Select(o => o.ToDto()).ToList();
    }
}

