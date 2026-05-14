namespace ECommerce.Domain.Entities;

public class Seller
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? BusinessRegistration { get; set; }
    public bool IsApproved { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
