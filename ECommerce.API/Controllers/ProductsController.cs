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
    private readonly IWebHostEnvironment _environment;

    public ProductsController(AppDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult<PagedProductsDto>> GetProducts([FromQuery] ProductQuery query)
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

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var totalCount = await products.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Min(page, totalPages);
        var result = await products
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
        return new PagedProductsDto(result.Select(p => p.ToDto()).ToList(), totalCount, page, pageSize, totalPages);
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

    [HttpGet("{id:int}/reviews")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews(int id)
    {
        var exists = await _db.Products.AnyAsync(p => p.Id == id);
        if (!exists)
        {
            return NotFound();
        }

        var reviews = await _db.Reviews
            .Where(r => r.ProductId == id)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return Ok(reviews.Any()
            ? reviews.Select(r => r.ToDto())
            : ApiMappings.MockReviewsForProduct(id));
    }



    [HttpPost("images")]
    [Authorize(Roles = "Seller,Admin")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<ProductImageUploadResponse>> UploadProductImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Choose an image file.");
        }

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(file.FileName);
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Only JPG, PNG, WEBP, and GIF images are allowed.");
        }

        if (file.Length > 5_000_000)
        {
            return BadRequest("Image must be 5 MB or smaller.");
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var uploadDirectory = Path.Combine(webRoot, "uploads", "products");
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream, cancellationToken);

        var imageUrl = $"/uploads/products/{fileName}";
        return Ok(new ProductImageUploadResponse(imageUrl));
    }


    [HttpPost]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct(ProductRequest request)
    {
        if (!await CanManageSellerAsync(request.SellerId))
        {
            return Forbid();
        }

        if (request.Price < 0 || request.Stock < 0)
        {
            return BadRequest("Price and stock cannot be negative.");
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

        if (request.Price < 0 || request.Stock < 0)
        {
            return BadRequest("Price and stock cannot be negative.");
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
