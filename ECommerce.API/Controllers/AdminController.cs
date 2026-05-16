using ECommerce.API.Contracts;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("users")]
    public async Task<IEnumerable<UserDto>> GetUsers()
    {
        var users = await _db.Users
            .IgnoreQueryFilters()
            .Include(u => u.Profile)
            .Include(u => u.Addresses)
            .AsNoTracking()
            .ToListAsync();

        return users.Select(u => u.ToDto()).ToList();
    }

    [HttpPatch("users/{id:int}/suspend")]
    public async Task<IActionResult> SetSuspended(int id, [FromQuery] bool suspended)
    {
        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        user.IsSuspended = suspended;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("users/{id:int}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        user.EmailConfirmed = true;
        user.IsSuspended = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> SoftDeleteUser(int id)
    {
        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        user.IsDeleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("sellers")]
    public async Task<IEnumerable<SellerDto>> GetSellers()
    {
        var sellers = await _db.Sellers
            .IgnoreQueryFilters()
            .Include(s => s.User)
            .AsNoTracking()
            .ToListAsync();

        return sellers.Select(s => s.ToDto()).ToList();
    }
    [HttpPatch("sellers/{id:int}/approve")]
    public async Task<IActionResult> ApproveSeller(int id, [FromQuery] bool approved)
    {
        var seller = await _db.Sellers.FirstOrDefaultAsync(s => s.Id == id);
        if (seller is null)
        {
            return NotFound();
        }

        seller.IsApproved = approved;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("orders")]
    public async Task<IEnumerable<OrderDto>> GetOrders()
    {
        var orders = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .Include(o => o.Payment)
            .Include(o => o.StatusHistory)
            .AsNoTracking()
            .ToListAsync();

        return orders.Select(o => o.ToDto()).ToList();
    }

    [HttpPost("banners")]
    public async Task<ActionResult<BannerDto>> CreateBanner(BannerRequest request)
    {
        var banner = new Banner { ImageUrl = request.ImageUrl, TargetUrl = request.TargetUrl, IsActive = request.IsActive, Title = request.Title };
        _db.Banners.Add(banner);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBanners), new { id = banner.Id }, banner.ToDto());
    }

    [HttpGet("banners")]
    public async Task<IEnumerable<BannerDto>> GetBanners()
    {
        var banners = await _db.Banners.AsNoTracking().ToListAsync();
        return banners.Select(b => b.ToDto()).ToList();
    }
}


