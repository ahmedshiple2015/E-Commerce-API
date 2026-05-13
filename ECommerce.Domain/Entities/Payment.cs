using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string GatewayTransactionId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    // Navigation Properties
    public Order Order { get; set; }
}
