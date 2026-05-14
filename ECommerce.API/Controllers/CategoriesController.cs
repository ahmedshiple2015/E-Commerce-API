using ECommerce.API.Contracts;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IEnumerable<CategoryDto>> GetCategories()
    {
        var categories = await _db.Categories.Include(c => c.Children).AsNoTracking().ToListAsync();
        return categories.Select(c => c.ToDto()).ToList();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryRequest request)
    {
        var category = new Category { Name = request.Name, Description = request.Description, ParentCategoryId = request.ParentCategoryId };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category.ToDto());
    }
}
