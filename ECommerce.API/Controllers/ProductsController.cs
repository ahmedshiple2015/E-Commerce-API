using ECommerce.API.Contracts;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] ProductQuery query)
    {
        var products = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            products = products.Where(p => p.Name.Contains(query.Search) || p.Description.Contains(query.Search));
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        if (query.MinPrice.HasValue)
        {
            products = products.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            products = products.Where(p => p.Price <= query.MaxPrice.Value);
        }

        if (query.MinRating.HasValue)
        {
            products = products.Where(p => p.Reviews.Any() && p.Reviews.Average(r => r.Rating) >= query.MinRating.Value);
        }

        var result = await products.AsNoTracking().ToListAsync();
        return result.Select(p => p.ToDto()).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? NotFound() : product.ToDto();
    }

    [HttpPost]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct(ProductRequest request)
    {
        if (!await CanManageSellerAsync(request.SellerId))
        {
            return Forbid();
        }

        var product = new Product
        {
            SellerId = request.SellerId,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            ImageUrl = request.ImageUrl,
            Images = request.Images?.Select(i => new ProductImage { ImageUrl = i.ImageUrl, SortOrder = i.SortOrder, IsPrimary = i.IsPrimary }).ToList() ?? new List<ProductImage>()
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> UpdateProduct(int id, ProductRequest request)
    {
        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        if (!await CanManageSellerAsync(product.SellerId) || (!IsAdmin && request.SellerId != product.SellerId))
        {
            return Forbid();
        }

        product.SellerId = request.SellerId;
        product.CategoryId = request.CategoryId;
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.ImageUrl = request.ImageUrl;
        product.Images.Clear();
        foreach (var image in request.Images ?? new List<ProductImageRequest>())
        {
            product.Images.Add(new ProductImage { ProductId = product.Id, ImageUrl = image.ImageUrl, SortOrder = image.SortOrder, IsPrimary = image.IsPrimary });
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        if (!await CanManageSellerAsync(product.SellerId))
        {
            return Forbid();
        }

        product.IsDeleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/reviews")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<ReviewDto>> AddReview(int id, ReviewRequest request)
    {
        if (!CanAccessUser(request.UserId))
        {
            return OwnershipForbidden();
        }

        if (id != request.ProductId || request.Rating is < 1 or > 5)
        {
            return BadRequest("Review product id must match and rating must be between 1 and 5.");
        }

        var purchased = await _db.Orders
            .Include(o => o.OrderItems)
            .AnyAsync(o => o.UserId == request.UserId && o.OrderItems.Any(i => i.ProductId == id));
        if (!purchased)
        {
            return BadRequest("Only users with purchase history can review this product.");
        }

        var review = new Review { ProductId = id, UserId = request.UserId, Rating = request.Rating, Comment = request.Comment };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id }, review.ToDto());
    }

    private async Task<bool> CanManageSellerAsync(int sellerId)
    {
        if (IsAdmin)
        {
            return true;
        }

        return await _db.Sellers.AnyAsync(s => s.Id == sellerId && s.UserId == CurrentUserId);
    }
}
