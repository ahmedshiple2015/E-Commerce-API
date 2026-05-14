using ECommerce.Domain.Enums;
using ECommerce.Domain.Entities;

namespace ECommerce.API.Contracts;

public record RegisterRequest(string Email, string Password, string? Phone, string? FullName, UserRole Role);
public record LoginRequest(string Email, string Password);
public record AuthResponse(int UserId, string Email, UserRole Role, string Token);

public record AddressRequest(string Label, string Line1, string? Line2, string City, string State, string PostalCode, string Country, bool IsDefaultShipping, bool IsDefaultBilling);
public record ProfileRequest(string FullName, string? Address, string? PaymentDetails);

public record CategoryRequest(string Name, string? Description, int? ParentCategoryId);
public record ProductImageRequest(string ImageUrl, int SortOrder, bool IsPrimary);
public record ProductRequest(int SellerId, int CategoryId, string Name, string Description, decimal Price, int Stock, string? ImageUrl, List<ProductImageRequest>? Images);
public record ProductQuery(string? Search, int? CategoryId, decimal? MinPrice, decimal? MaxPrice, int? MinRating);
public record ReviewRequest(int UserId, int ProductId, int Rating, string? Comment);

public record CartItemRequest(int ProductId, int Quantity, string? GuestAccessToken = null);
public record CheckoutRequest(int? UserId, string? SessionId, string ShippingAddress, PaymentMethod PaymentMethod, string? GuestAccessToken = null);
public record OrderStatusRequest(OrderStatus Status, string? Notes);
public record PaymentRequest(PaymentMethod PaymentMethod, string? GatewayTransactionId, decimal Amount);
public record PaymentWebhookRequest(int OrderId, PaymentMethod PaymentMethod, string GatewayTransactionId, string Status, decimal Amount);
public record CreatePaymentIntentRequest(int OrderId, string? Currency, string? GuestAccessToken = null);
public record CreatePaymentIntentResponse(string PaymentIntentId, string ClientSecret, long Amount, string Currency, string PublishableKey);

public record SellerRequest(int UserId, string StoreName, string? BusinessRegistration);
public record BannerRequest(string ImageUrl, string? TargetUrl, bool IsActive, string? Title);

public record UserDto(int Id, string Email, string? PhoneNumber, UserRole Role, bool EmailConfirmed, bool IsSuspended, ProfileDto? Profile, IEnumerable<AddressDto> Addresses);
public record ProfileDto(int Id, string FullName, string? Address, string? PaymentDetails);
public record AddressDto(int Id, string Label, string Line1, string? Line2, string City, string State, string PostalCode, string Country, bool IsDefaultShipping, bool IsDefaultBilling);
public record SellerDto(int Id, int UserId, string StoreName, string? BusinessRegistration, bool IsApproved);
public record CategoryDto(int Id, int? ParentCategoryId, string Name, string? Description, IEnumerable<CategoryDto> Children);
public record ProductImageDto(int Id, string ImageUrl, int SortOrder, bool IsPrimary);
public record ProductDto(int Id, int SellerId, int CategoryId, string Name, string Description, decimal Price, int Stock, string? ImageUrl, CategoryDto? Category, IEnumerable<ProductImageDto> Images, double AverageRating, int ReviewCount);
public record ReviewDto(int Id, int ProductId, int UserId, int Rating, string? Comment, DateTime CreatedAt);
public record CartDto(int Id, int? UserId, string? SessionId, string? GuestAccessToken, DateTime UpdatedAt, IEnumerable<CartItemDto> Items, decimal Subtotal);
public record CartItemDto(int Id, int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
public record OrderDto(int Id, int? UserId, string? GuestAccessToken, decimal TotalAmount, decimal TaxAmount, decimal ShippingAmount, OrderStatus Status, string ShippingAddress, DateTime CreatedAt, IEnumerable<OrderItemDto> Items, PaymentDto? Payment, IEnumerable<OrderStatusHistoryDto> StatusHistory);
public record OrderItemDto(int Id, int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
public record OrderStatusHistoryDto(int Id, OrderStatus Status, string? Notes, DateTime ChangedAt);
public record PaymentDto(int Id, int OrderId, string? GatewayTransactionId, PaymentMethod PaymentMethod, PaymentStatus PaymentStatus, decimal Amount, DateTime CreatedAt);
public record BannerDto(int Id, string ImageUrl, string? TargetUrl, bool IsActive, string? Title);

public static class ApiMappings
{
    public static UserDto ToDto(this ApplicationUser user)
    {
        return new UserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.PhoneNumber,
            user.Role,
            user.EmailConfirmed,
            user.IsSuspended,
            user.Profile?.ToDto(),
            user.Addresses.Select(a => a.ToDto()));
    }

    public static ProfileDto ToDto(this UserProfile profile)
    {
        return new ProfileDto(profile.Id, profile.FullName, profile.Address, profile.PaymentDetails);
    }

    public static AddressDto ToDto(this Address address)
    {
        return new AddressDto(
            address.Id,
            address.Label,
            address.Line1,
            address.Line2,
            address.City,
            address.State,
            address.PostalCode,
            address.Country,
            address.IsDefaultShipping,
            address.IsDefaultBilling);
    }

    public static SellerDto ToDto(this Seller seller)
    {
        return new SellerDto(seller.Id, seller.UserId, seller.StoreName, seller.BusinessRegistration, seller.IsApproved);
    }

    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto(
            category.Id,
            category.ParentCategoryId,
            category.Name,
            category.Description,
            category.Children.Select(c => c.ToDto()));
    }

    public static ProductImageDto ToDto(this ProductImage image)
    {
        return new ProductImageDto(image.Id, image.ImageUrl, image.SortOrder, image.IsPrimary);
    }

    public static ProductDto ToDto(this Product product)
    {
        var reviewCount = product.Reviews.Count;
        var averageRating = reviewCount == 0 ? 0 : product.Reviews.Average(r => r.Rating);

        return new ProductDto(
            product.Id,
            product.SellerId,
            product.CategoryId,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.ImageUrl,
            product.Category?.ToDto(),
            product.Images.OrderBy(i => i.SortOrder).Select(i => i.ToDto()),
            averageRating,
            reviewCount);
    }

    public static ReviewDto ToDto(this Review review)
    {
        return new ReviewDto(review.Id, review.ProductId, review.UserId, review.Rating, review.Comment, review.CreatedAt);
    }

    public static CartDto ToDto(this Cart cart)
    {
        var items = cart.Items.Select(i => i.ToDto()).ToList();
        return new CartDto(cart.Id, cart.UserId, cart.SessionId, cart.GuestAccessToken, cart.UpdatedAt, items, items.Sum(i => i.LineTotal));
    }

    public static CartItemDto ToDto(this CartItem item)
    {
        var unitPrice = item.Product?.Price ?? 0;
        return new CartItemDto(item.Id, item.ProductId, item.Product?.Name ?? string.Empty, unitPrice, item.Quantity, unitPrice * item.Quantity);
    }

    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto(
            order.Id,
            order.UserId,
            order.GuestAccessToken,
            order.TotalAmount,
            order.TaxAmount,
            order.ShippingAmount,
            order.Status,
            order.ShippingAddress,
            order.CreatedAt,
            order.OrderItems.Select(i => i.ToDto()),
            order.Payment?.ToDto(),
            order.StatusHistory.OrderBy(h => h.ChangedAt).Select(h => h.ToDto()));
    }

    public static OrderItemDto ToDto(this OrderItem item)
    {
        return new OrderItemDto(item.Id, item.ProductId, item.Product?.Name ?? string.Empty, item.Quantity, item.UnitPrice, item.UnitPrice * item.Quantity);
    }

    public static OrderStatusHistoryDto ToDto(this OrderStatusHistory history)
    {
        return new OrderStatusHistoryDto(history.Id, history.Status, history.Notes, history.ChangedAt);
    }

    public static PaymentDto ToDto(this Payment payment)
    {
        return new PaymentDto(payment.Id, payment.OrderId, payment.GatewayTransactionId, payment.PaymentMethod, payment.PaymentStatus, payment.Amount, payment.CreatedAt);
    }

    public static BannerDto ToDto(this Banner banner)
    {
        return new BannerDto(banner.Id, banner.ImageUrl, banner.TargetUrl, banner.IsActive, banner.Title);
    }
}
