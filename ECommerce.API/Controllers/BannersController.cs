using ECommerce.API.Contracts;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BannersController : ControllerBase
{
    private readonly AppDbContext _db;

    public BannersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IEnumerable<BannerDto>> GetActiveBanners()
    {
        var banners = await _db.Banners
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.Id)
            .AsNoTracking()
            .ToListAsync();

        return banners.Select(b => b.ToDto()).ToList();
    }
}
