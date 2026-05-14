using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Domain.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public UserRole Role { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsSuspended { get; set; }

    public UserProfile? Profile { get; set; }
    public Seller? Seller { get; set; }
    public Cart? Cart { get; set; }
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
