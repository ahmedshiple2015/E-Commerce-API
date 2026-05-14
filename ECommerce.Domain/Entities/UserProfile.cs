namespace ECommerce.Domain.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PaymentDetails { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
