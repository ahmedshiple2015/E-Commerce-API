namespace ECommerce.Domain.Entities;

public class Cart
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string SessionId { get; set; }

    // Navigation Properties
    public User User { get; set; }
    public ICollection<CartItem> Items { get; set; }
}
