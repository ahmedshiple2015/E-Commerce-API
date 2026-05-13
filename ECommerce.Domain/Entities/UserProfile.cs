namespace ECommerce.Domain.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string Address { get; set; }
    public string PaymentDetails { get; set; }

    // Navigation Properties
    public User User { get; set; }
}
