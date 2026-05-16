using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260516173000_SeedMockProducts")]
public partial class SeedMockProducts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Id] = 900001)
            BEGIN
                SET IDENTITY_INSERT [Users] ON;
                INSERT INTO [Users] (
                    [Id], [Role], [IsDeleted], [IsSuspended], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
                    [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber],
                    [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]
                )
                VALUES (
                    900001, N'Seller', 0, 0, N'mock.seller@ecommerce.local', N'MOCK.SELLER@ECOMMERCE.LOCAL',
                    N'mock.seller@ecommerce.local', N'MOCK.SELLER@ECOMMERCE.LOCAL', 1, NULL,
                    N'6c1e6e09-8b93-4560-9c69-1f6b6f221001', N'6c1e6e09-8b93-4560-9c69-1f6b6f221002',
                    NULL, 0, 0, NULL, 1, 0
                );
                SET IDENTITY_INSERT [Users] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [UserProfiles] WHERE [Id] = 900001)
            BEGIN
                SET IDENTITY_INSERT [UserProfiles] ON;
                INSERT INTO [UserProfiles] ([Id], [UserId], [FullName], [Address], [PaymentDetails])
                VALUES (900001, 900001, N'Mock Catalog Seller', NULL, NULL);
                SET IDENTITY_INSERT [UserProfiles] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [Sellers] WHERE [Id] = 900001)
            BEGIN
                SET IDENTITY_INSERT [Sellers] ON;
                INSERT INTO [Sellers] ([Id], [UserId], [StoreName], [BusinessRegistration], [IsApproved])
                VALUES (900001, 900001, N'Commerce Mock Store', N'MOCK-SEED-001', 1);
                SET IDENTITY_INSERT [Sellers] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = 900001)
            BEGIN
                SET IDENTITY_INSERT [Categories] ON;
                INSERT INTO [Categories] ([Id], [ParentCategoryId], [Name], [Description]) VALUES
                (900001, NULL, N'Electronics', N'Devices, audio, cameras, and smart accessories.'),
                (900002, NULL, N'Fashion', N'Clothing, shoes, watches, and everyday carry.'),
                (900003, NULL, N'Home & Kitchen', N'Home comfort, kitchen tools, and workspace essentials.'),
                (900004, NULL, N'Beauty & Wellness', N'Personal care, grooming, and wellness products.'),
                (900005, NULL, N'Sports & Outdoors', N'Fitness, travel, and outdoor gear.');
                SET IDENTITY_INSERT [Categories] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 900001)
            BEGIN
                SET IDENTITY_INSERT [Products] ON;
                INSERT INTO [Products] ([Id], [SellerId], [CategoryId], [Name], [Description], [Price], [Stock], [ImageUrl], [IsDeleted]) VALUES
                (900001, 900001, 900001, N'Aurora Wireless Headphones', N'Comfortable over-ear wireless headphones with deep bass and long battery life.', 129.99, 42, N'https://placehold.co/600x400?text=Aurora+Headphones', 0),
                (900002, 900001, 900001, N'Pulse Smart Watch', N'Lightweight smart watch with activity tracking, notifications, and heart-rate monitoring.', 89.50, 55, N'https://placehold.co/600x400?text=Pulse+Smart+Watch', 0),
                (900003, 900001, 900001, N'Nova Bluetooth Speaker', N'Portable speaker with crisp sound, water resistance, and all-day playtime.', 59.99, 73, N'https://placehold.co/600x400?text=Nova+Speaker', 0),
                (900004, 900001, 900001, N'Volt USB-C Power Bank', N'Compact 20000mAh power bank with fast USB-C charging for phones and tablets.', 44.99, 88, N'https://placehold.co/600x400?text=Volt+Power+Bank', 0),
                (900005, 900001, 900001, N'Focus 4K Webcam', N'Sharp 4K webcam with autofocus and clear microphone for meetings and streaming.', 74.00, 31, N'https://placehold.co/600x400?text=Focus+4K+Webcam', 0),
                (900006, 900001, 900001, N'Mini Mechanical Keyboard', N'Compact mechanical keyboard with tactile switches and customizable lighting.', 69.99, 64, N'https://placehold.co/600x400?text=Mini+Keyboard', 0),
                (900007, 900001, 900002, N'Everyday Cotton Hoodie', N'Soft cotton-blend hoodie designed for everyday comfort and clean layering.', 39.99, 120, N'https://placehold.co/600x400?text=Cotton+Hoodie', 0),
                (900008, 900001, 900002, N'Classic Denim Jacket', N'Durable denim jacket with a timeless cut and practical front pockets.', 64.50, 46, N'https://placehold.co/600x400?text=Denim+Jacket', 0),
                (900009, 900001, 900002, N'Urban Runner Sneakers', N'Breathable sneakers with cushioned soles for casual wear and light training.', 79.99, 67, N'https://placehold.co/600x400?text=Runner+Sneakers', 0),
                (900010, 900001, 900002, N'Canvas Travel Backpack', N'Roomy backpack with laptop sleeve, water-resistant canvas, and padded straps.', 54.99, 39, N'https://placehold.co/600x400?text=Travel+Backpack', 0),
                (900011, 900001, 900002, N'Minimal Leather Wallet', N'Slim leather wallet with RFID-blocking lining and quick-access card slots.', 24.99, 96, N'https://placehold.co/600x400?text=Leather+Wallet', 0),
                (900012, 900001, 900002, N'Silver Chrono Watch', N'Elegant stainless-steel watch with chronograph styling and durable mineral glass.', 119.00, 28, N'https://placehold.co/600x400?text=Chrono+Watch', 0),
                (900013, 900001, 900003, N'Ceramic Pour-Over Set', N'Beautiful ceramic pour-over coffee set for smooth, slow-brewed mornings.', 34.99, 51, N'https://placehold.co/600x400?text=Pour+Over+Set', 0),
                (900014, 900001, 900003, N'Stainless Chef Knife', N'Balanced stainless-steel chef knife for precise slicing, chopping, and prep work.', 49.99, 43, N'https://placehold.co/600x400?text=Chef+Knife', 0),
                (900015, 900001, 900003, N'Bamboo Cutting Board', N'Large bamboo cutting board with juice groove and smooth food-safe finish.', 22.50, 76, N'https://placehold.co/600x400?text=Bamboo+Board', 0),
                (900016, 900001, 900003, N'Nordic Desk Lamp', N'Adjustable LED desk lamp with warm light modes for reading and focused work.', 37.99, 58, N'https://placehold.co/600x400?text=Desk+Lamp', 0),
                (900017, 900001, 900003, N'Cozy Knit Throw Blanket', N'Soft knit throw blanket that adds warmth and texture to any room.', 29.99, 84, N'https://placehold.co/600x400?text=Knit+Blanket', 0),
                (900018, 900001, 900003, N'Aroma Diffuser', N'Quiet essential-oil diffuser with ambient light and automatic shutoff.', 32.00, 69, N'https://placehold.co/600x400?text=Aroma+Diffuser', 0),
                (900019, 900001, 900004, N'Hydrating Face Serum', N'Lightweight serum formulated to hydrate skin and support a natural glow.', 27.99, 92, N'https://placehold.co/600x400?text=Face+Serum', 0),
                (900020, 900001, 900004, N'Botanical Body Wash', N'Gentle botanical body wash with a fresh scent and moisturizing feel.', 13.99, 140, N'https://placehold.co/600x400?text=Body+Wash', 0),
                (900021, 900001, 900004, N'Matte Grooming Clay', N'Strong-hold styling clay with a matte finish for textured hair looks.', 18.50, 75, N'https://placehold.co/600x400?text=Grooming+Clay', 0),
                (900022, 900001, 900004, N'Wellness Yoga Mat', N'Non-slip yoga mat with comfortable cushioning for studio or home practice.', 35.99, 60, N'https://placehold.co/600x400?text=Yoga+Mat', 0),
                (900023, 900001, 900004, N'Rechargeable Toothbrush', N'Electric toothbrush with gentle modes, timer, and long-lasting rechargeable battery.', 41.99, 47, N'https://placehold.co/600x400?text=Toothbrush', 0),
                (900024, 900001, 900004, N'Compact Massage Gun', N'Portable massage gun with multiple speeds for recovery and muscle relaxation.', 86.00, 24, N'https://placehold.co/600x400?text=Massage+Gun', 0),
                (900025, 900001, 900005, N'Insulated Water Bottle', N'Double-wall insulated bottle that keeps drinks cold or hot for hours.', 21.99, 110, N'https://placehold.co/600x400?text=Water+Bottle', 0),
                (900026, 900001, 900005, N'Adjustable Dumbbell Pair', N'Space-saving adjustable dumbbells for strength workouts at home.', 149.99, 18, N'https://placehold.co/600x400?text=Dumbbells', 0),
                (900027, 900001, 900005, N'Trail Hiking Backpack', N'Lightweight hiking pack with breathable back panel and hydration pocket.', 72.00, 36, N'https://placehold.co/600x400?text=Hiking+Pack', 0),
                (900028, 900001, 900005, N'Compact Camping Lantern', N'Bright rechargeable lantern with hanging hook and emergency flashing mode.', 26.99, 80, N'https://placehold.co/600x400?text=Camping+Lantern', 0),
                (900029, 900001, 900005, N'Quick-Dry Sport Towel', N'Absorbent microfiber towel that packs small for gym, beach, or travel.', 16.99, 101, N'https://placehold.co/600x400?text=Sport+Towel', 0),
                (900030, 900001, 900005, N'All-Weather Fitness Band Set', N'Resistance band set with multiple strengths for stretching and workouts.', 23.99, 93, N'https://placehold.co/600x400?text=Fitness+Bands', 0);
                SET IDENTITY_INSERT [Products] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [ProductImages] WHERE [Id] = 900001)
            BEGIN
                SET IDENTITY_INSERT [ProductImages] ON;
                INSERT INTO [ProductImages] ([Id], [ProductId], [ImageUrl], [SortOrder], [IsPrimary]) VALUES
                (900001, 900001, N'https://placehold.co/600x400?text=Aurora+Headphones', 0, 1),
                (900002, 900002, N'https://placehold.co/600x400?text=Pulse+Smart+Watch', 0, 1),
                (900003, 900003, N'https://placehold.co/600x400?text=Nova+Speaker', 0, 1),
                (900004, 900004, N'https://placehold.co/600x400?text=Volt+Power+Bank', 0, 1),
                (900005, 900005, N'https://placehold.co/600x400?text=Focus+4K+Webcam', 0, 1),
                (900006, 900006, N'https://placehold.co/600x400?text=Mini+Keyboard', 0, 1),
                (900007, 900007, N'https://placehold.co/600x400?text=Cotton+Hoodie', 0, 1),
                (900008, 900008, N'https://placehold.co/600x400?text=Denim+Jacket', 0, 1),
                (900009, 900009, N'https://placehold.co/600x400?text=Runner+Sneakers', 0, 1),
                (900010, 900010, N'https://placehold.co/600x400?text=Travel+Backpack', 0, 1),
                (900011, 900011, N'https://placehold.co/600x400?text=Leather+Wallet', 0, 1),
                (900012, 900012, N'https://placehold.co/600x400?text=Chrono+Watch', 0, 1),
                (900013, 900013, N'https://placehold.co/600x400?text=Pour+Over+Set', 0, 1),
                (900014, 900014, N'https://placehold.co/600x400?text=Chef+Knife', 0, 1),
                (900015, 900015, N'https://placehold.co/600x400?text=Bamboo+Board', 0, 1),
                (900016, 900016, N'https://placehold.co/600x400?text=Desk+Lamp', 0, 1),
                (900017, 900017, N'https://placehold.co/600x400?text=Knit+Blanket', 0, 1),
                (900018, 900018, N'https://placehold.co/600x400?text=Aroma+Diffuser', 0, 1),
                (900019, 900019, N'https://placehold.co/600x400?text=Face+Serum', 0, 1),
                (900020, 900020, N'https://placehold.co/600x400?text=Body+Wash', 0, 1),
                (900021, 900021, N'https://placehold.co/600x400?text=Grooming+Clay', 0, 1),
                (900022, 900022, N'https://placehold.co/600x400?text=Yoga+Mat', 0, 1),
                (900023, 900023, N'https://placehold.co/600x400?text=Toothbrush', 0, 1),
                (900024, 900024, N'https://placehold.co/600x400?text=Massage+Gun', 0, 1),
                (900025, 900025, N'https://placehold.co/600x400?text=Water+Bottle', 0, 1),
                (900026, 900026, N'https://placehold.co/600x400?text=Dumbbells', 0, 1),
                (900027, 900027, N'https://placehold.co/600x400?text=Hiking+Pack', 0, 1),
                (900028, 900028, N'https://placehold.co/600x400?text=Camping+Lantern', 0, 1),
                (900029, 900029, N'https://placehold.co/600x400?text=Sport+Towel', 0, 1),
                (900030, 900030, N'https://placehold.co/600x400?text=Fitness+Bands', 0, 1);
                SET IDENTITY_INSERT [ProductImages] OFF;
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM [ProductImages] WHERE [Id] BETWEEN 900001 AND 900030;
            DELETE FROM [Products] WHERE [Id] BETWEEN 900001 AND 900030;
            DELETE FROM [Categories] WHERE [Id] BETWEEN 900001 AND 900005;
            DELETE FROM [Sellers] WHERE [Id] = 900001;
            DELETE FROM [UserProfiles] WHERE [Id] = 900001;
            DELETE FROM [Users] WHERE [Id] = 900001;
            """);
    }
}
