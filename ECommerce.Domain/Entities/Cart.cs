namespace ECommerce.Domain.Entities;

public class Cart
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? SessionId { get; set; }
    public string? GuestAccessToken { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
