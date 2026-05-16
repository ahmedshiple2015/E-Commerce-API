using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260516183000_SeedMockUsers")]
public partial class SeedMockUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [NormalizedName] = N'CUSTOMER')
            BEGIN
                SET IDENTITY_INSERT [Roles] ON;
                INSERT INTO [Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                VALUES (900101, N'Customer', N'CUSTOMER', N'2ad6fa6d-2b18-40b0-a7d3-mock-customer-role');
                SET IDENTITY_INSERT [Roles] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [NormalizedName] = N'SELLER')
            BEGIN
                SET IDENTITY_INSERT [Roles] ON;
                INSERT INTO [Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                VALUES (900102, N'Seller', N'SELLER', N'2ad6fa6d-2b18-40b0-a7d3-mock-seller-role');
                SET IDENTITY_INSERT [Roles] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [NormalizedName] = N'ADMIN')
            BEGIN
                SET IDENTITY_INSERT [Roles] ON;
                INSERT INTO [Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                VALUES (900103, N'Admin', N'ADMIN', N'2ad6fa6d-2b18-40b0-a7d3-mock-admin-role');
                SET IDENTITY_INSERT [Roles] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Id] = 900101)
            BEGIN
                SET IDENTITY_INSERT [Users] ON;
                INSERT INTO [Users] (
                    [Id], [Role], [IsDeleted], [IsSuspended], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
                    [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber],
                    [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]
                )
                VALUES (
                    900101, N'Customer', 0, 0, N'mock.customer@ecommerce.local', N'MOCK.CUSTOMER@ECOMMERCE.LOCAL',
                    N'mock.customer@ecommerce.local', N'MOCK.CUSTOMER@ECOMMERCE.LOCAL', 1,
                    N'AQAAAAIAAYagAAAAECo/rZjN8EfFaHBLElVpGiiwKSVhiTfixyHtcwe6bAbgMvliotLWx11uI9TbhvJekQ==',
                    N'6c1e6e09-8b93-4560-9c69-1f6b6f222101', N'6c1e6e09-8b93-4560-9c69-1f6b6f222102',
                    N'+10000000001', 0, 0, NULL, 1, 0
                );
                SET IDENTITY_INSERT [Users] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [UserProfiles] WHERE [Id] = 900101)
            BEGIN
                SET IDENTITY_INSERT [UserProfiles] ON;
                INSERT INTO [UserProfiles] ([Id], [UserId], [FullName], [Address], [PaymentDetails])
                VALUES (900101, 900101, N'Mock Customer', N'123 Demo Customer Street', NULL);
                SET IDENTITY_INSERT [UserProfiles] OFF;
            END

            IF EXISTS (SELECT 1 FROM [Users] WHERE [Id] = 900001)
            BEGIN
                UPDATE [Users]
                SET [Role] = N'Seller',
                    [EmailConfirmed] = 1,
                    [PasswordHash] = N'AQAAAAIAAYagAAAAEESKtYlkXOX3R6NUrma8vLPBIxbFvTAULJedzaWZldaXH0q0oa4OOlIns+MHNwmBOg==',
                    [SecurityStamp] = COALESCE([SecurityStamp], N'6c1e6e09-8b93-4560-9c69-1f6b6f222201')
                WHERE [Id] = 900001;
            END

            IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Id] = 900102)
            BEGIN
                SET IDENTITY_INSERT [Users] ON;
                INSERT INTO [Users] (
                    [Id], [Role], [IsDeleted], [IsSuspended], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
                    [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber],
                    [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]
                )
                VALUES (
                    900102, N'Admin', 0, 0, N'mock.admin@ecommerce.local', N'MOCK.ADMIN@ECOMMERCE.LOCAL',
                    N'mock.admin@ecommerce.local', N'MOCK.ADMIN@ECOMMERCE.LOCAL', 1,
                    N'AQAAAAIAAYagAAAAEOIuZ9BWtUIzOg2oK1QjpTHdpcXbyY4lObfSQX0riXZFjgMOiGLJwoOsghy6NlzAOQ==',
                    N'6c1e6e09-8b93-4560-9c69-1f6b6f222301', N'6c1e6e09-8b93-4560-9c69-1f6b6f222302',
                    N'+10000000003', 0, 0, NULL, 1, 0
                );
                SET IDENTITY_INSERT [Users] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [UserProfiles] WHERE [Id] = 900102)
            BEGIN
                SET IDENTITY_INSERT [UserProfiles] ON;
                INSERT INTO [UserProfiles] ([Id], [UserId], [FullName], [Address], [PaymentDetails])
                VALUES (900102, 900102, N'Mock Admin', N'1 Admin Demo Plaza', NULL);
                SET IDENTITY_INSERT [UserProfiles] OFF;
            END

            DECLARE @CustomerRoleId int = (SELECT TOP 1 [Id] FROM [Roles] WHERE [NormalizedName] = N'CUSTOMER');
            DECLARE @SellerRoleId int = (SELECT TOP 1 [Id] FROM [Roles] WHERE [NormalizedName] = N'SELLER');
            DECLARE @AdminRoleId int = (SELECT TOP 1 [Id] FROM [Roles] WHERE [NormalizedName] = N'ADMIN');

            IF NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [UserId] = 900101 AND [RoleId] = @CustomerRoleId)
                INSERT INTO [UserRoles] ([UserId], [RoleId]) VALUES (900101, @CustomerRoleId);

            IF NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [UserId] = 900001 AND [RoleId] = @SellerRoleId)
                INSERT INTO [UserRoles] ([UserId], [RoleId]) VALUES (900001, @SellerRoleId);

            IF NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [UserId] = 900102 AND [RoleId] = @AdminRoleId)
                INSERT INTO [UserRoles] ([UserId], [RoleId]) VALUES (900102, @AdminRoleId);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DECLARE @CustomerRoleId int = (SELECT TOP 1 [Id] FROM [Roles] WHERE [NormalizedName] = N'CUSTOMER');
            DECLARE @SellerRoleId int = (SELECT TOP 1 [Id] FROM [Roles] WHERE [NormalizedName] = N'SELLER');
            DECLARE @AdminRoleId int = (SELECT TOP 1 [Id] FROM [Roles] WHERE [NormalizedName] = N'ADMIN');

            DELETE FROM [UserRoles] WHERE [UserId] IN (900001, 900101, 900102) AND [RoleId] IN (@CustomerRoleId, @SellerRoleId, @AdminRoleId);
            DELETE FROM [UserProfiles] WHERE [Id] IN (900101, 900102);
            DELETE FROM [Users] WHERE [Id] IN (900101, 900102);
            UPDATE [Users] SET [PasswordHash] = NULL WHERE [Id] = 900001 AND [Email] = N'mock.seller@ecommerce.local';
            """);
    }
}
