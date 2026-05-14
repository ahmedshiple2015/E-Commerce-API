namespace ECommerce.Domain.Entities;

public class Banner
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public bool IsActive { get; set; }
    public string? Title { get; set; }
}
