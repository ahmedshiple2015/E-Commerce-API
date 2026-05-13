using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Seller> Sellers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Query Filter for Soft Delete
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

        // Fluent API Configurations

        // Context 1: Identity & Seller
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>();

            // 1-to-1 with UserProfile
            entity.HasOne(u => u.Profile)
                  .WithOne(p => p.User)
                  .HasForeignKey<UserProfile>(p => p.UserId);

            // 1-to-1 with Seller
            entity.HasOne(u => u.Seller)
                  .WithOne(s => s.User)
                  .HasForeignKey<Seller>(s => s.UserId);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Seller>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StoreName).IsRequired();
        });

        // Context 2: Catalog & Content
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2); // DECIMAL(18,2)
            
            entity.HasOne(p => p.Seller)
                  .WithMany(s => s.Products)
                  .HasForeignKey(p => p.SellerId);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(r => r.Product)
                  .WithMany(p => p.Reviews)
                  .HasForeignKey(r => r.ProductId);

            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(r => r.UserId);
        });

        // Context 3: Transactions
        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(w => w.User)
                  .WithMany(u => u.WishlistItems)
                  .HasForeignKey(w => w.UserId);

            entity.HasOne(w => w.Product)
                  .WithMany(p => p.WishlistItems)
                  .HasForeignKey(w => w.ProductId);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id);
            // 1-to-1 with User (optional for guests)
            entity.HasOne(c => c.User)
                  .WithOne(u => u.Cart)
                  .HasForeignKey<Cart>(c => c.UserId)
                  .IsRequired(false);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(ci => ci.Cart)
                  .WithMany(c => c.Items)
                  .HasForeignKey(ci => ci.CartId);

            entity.HasOne(ci => ci.Product)
                  .WithMany(p => p.CartItems)
                  .HasForeignKey(ci => ci.ProductId);
        });

        // Context 4: Orders & Fulfillment
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2); // DECIMAL(18,2)
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasOne(o => o.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(o => o.UserId)
                  .IsRequired(false);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2); // DECIMAL(18,2)

            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId);

            entity.HasOne(oi => oi.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(oi => oi.ProductId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaymentMethod).HasConversion<string>();
            entity.Property(e => e.PaymentStatus).HasConversion<string>();

            entity.HasOne(p => p.Order)
                  .WithOne(o => o.Payment)
                  .HasForeignKey<Payment>(p => p.OrderId);
        });
    }
}
