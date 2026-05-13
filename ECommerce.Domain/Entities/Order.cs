using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string ShippingAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public User User { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public Payment Payment { get; set; }
}
