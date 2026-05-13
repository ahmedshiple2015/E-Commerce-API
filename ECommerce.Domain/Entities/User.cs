using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation Properties
    public UserProfile Profile { get; set; }
    public Seller Seller { get; set; }
    public Cart Cart { get; set; }
    public ICollection<Order> Orders { get; set; }
    public ICollection<Review> Reviews { get; set; }
    public ICollection<WishlistItem> WishlistItems { get; set; }
}
