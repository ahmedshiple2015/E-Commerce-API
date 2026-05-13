namespace ECommerce.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; }

    // Navigation Properties
    public Seller Seller { get; set; }
    public Category Category { get; set; }
    public ICollection<Review> Reviews { get; set; }
    public ICollection<CartItem> CartItems { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<WishlistItem> WishlistItems { get; set; }
}
