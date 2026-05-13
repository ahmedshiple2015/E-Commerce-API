namespace ECommerce.Domain.Entities;

public class Seller
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StoreName { get; set; }
    public string BusinessRegistration { get; set; }

    // Navigation Properties
    public User User { get; set; }
    public ICollection<Product> Products { get; set; }
}
