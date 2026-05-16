using ECommerce.API.Contracts;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id:int}/profile")]
    public async Task<ActionResult<UserDto>> GetProfile(int id)
    {
        if (!CanAccessUser(id))
        {
            return OwnershipForbidden();
        }

        var user = await _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Addresses)
            .Include(u => u.WishlistItems).ThenInclude(w => w.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? NotFound() : user.ToDto();
    }

    [HttpPut("{id:int}/profile")]
    public async Task<IActionResult> UpdateProfile(int id, ProfileRequest request)
    {
        if (!CanAccessUser(id))
        {
            return OwnershipForbidden();
        }

        var user = await _db.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        user.Profile ??= new UserProfile { UserId = id };
        user.Profile.FullName = request.FullName;
        user.Profile.Address = request.Address;
        user.Profile.PaymentDetails = request.PaymentDetails;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/addresses")]
    public async Task<ActionResult<AddressDto>> AddAddress(int id, AddressRequest request)
    {
        if (!CanAccessUser(id))
        {
            return OwnershipForbidden();
        }

        if (!await _db.Users.AnyAsync(u => u.Id == id))
        {
            return NotFound();
        }

        var address = new Address
        {
            UserId = id,
            Label = request.Label,
            Line1 = request.Line1,
            Line2 = request.Line2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            IsDefaultBilling = request.IsDefaultBilling,
            IsDefaultShipping = request.IsDefaultShipping
        };

        if (address.IsDefaultShipping)
        {
            await _db.Addresses
                .Where(a => a.UserId == id && a.IsDefaultShipping)
                .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.IsDefaultShipping, false), cancellationToken: default);
        }

        if (address.IsDefaultBilling)
        {
            await _db.Addresses
                .Where(a => a.UserId == id && a.IsDefaultBilling)
                .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.IsDefaultBilling, false), cancellationToken: default);
        }

        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProfile), new { id }, address.ToDto());
    }

    [HttpPatch("{id:int}/addresses/{addressId:int}/default-shipping")]
    public async Task<IActionResult> SetDefaultShippingAddress(int id, int addressId)
    {
        if (!CanAccessUser(id))
        {
            return OwnershipForbidden();
        }

        var addresses = await _db.Addresses.Where(a => a.UserId == id).ToListAsync();
        if (!addresses.Any(a => a.Id == addressId))
        {
            return NotFound();
        }

        foreach (var address in addresses)
        {
            address.IsDefaultShipping = address.Id == addressId;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}/addresses/{addressId:int}")]
    public async Task<IActionResult> DeleteAddress(int id, int addressId)
    {
        if (!CanAccessUser(id))
        {
            return OwnershipForbidden();
        }

        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.UserId == id && a.Id == addressId);
        if (address is null)
        {
            return NotFound();
        }

        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();

        if (address.IsDefaultShipping)
        {
            var nextAddress = await _db.Addresses
                .Where(a => a.UserId == id)
                .OrderBy(a => a.Id)
                .FirstOrDefaultAsync();

            if (nextAddress is not null)
            {
                nextAddress.IsDefaultShipping = true;
                await _db.SaveChangesAsync();
            }
        }

        return NoContent();
    }

    [HttpGet("{id:int}/wishlist")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetWishlist(int id)
    {
        if (!CanAccessUser(id))
        {
            return OwnershipForbidden();
        }

        if (!await _db.Users.AnyAsync(u => u.Id == id))
        {
            return NotFound();
        }

        var products = await _db.WishlistItems
            .Where(w => w.UserId == id)
            .Include(w => w.Product)
                .ThenInclude(p => p.Category)
            .Include(w => w.Product)
                .ThenInclude(p => p.Images)
            .Include(w => w.Product)
                .ThenInclude(p => p.Reviews)
            .AsNoTracking()
            .Select(w => w.Product)
            .ToListAsync();

        return products.Select(p => p.ToDto()).ToList();
    }

    [HttpPost("{id:int}/wishlist/{productId:int}")]
    public async Task<IActionResult> AddWishlistItem(int id, int productId)
    {
        if (!CanAccessUser(id))
        {
            return OwnershipForbidden();
        }

        if (!await _db.Users.AnyAsync(u => u.Id == id) || !await _db.Products.AnyAsync(p => p.Id == productId))
        {
            return NotFound();
        }

        if (!await _db.WishlistItems.AnyAsync(w => w.UserId == id && w.ProductId == productId))
        {
            _db.WishlistItems.Add(new WishlistItem { UserId = id, ProductId = productId });
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}/wishlist/{productId:int}")]
    public async Task<IActionResult> RemoveWishlistItem(int id, int productId)
    {
        if (!CanAccessUser(id))
        {
            return OwnershipForbidden();
        }

        var item = await _db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == id && w.ProductId == productId);
        if (item is null)
        {
            return NotFound();
        }

        _db.WishlistItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
