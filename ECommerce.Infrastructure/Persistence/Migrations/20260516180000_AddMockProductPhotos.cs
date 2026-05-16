using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260516180000_AddMockProductPhotos")]
public partial class AddMockProductPhotos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE [Products]
            SET [ImageUrl] = CASE [Id]
                WHEN 900001 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/apple-airpods-max-silver/thumbnail.webp'
                WHEN 900002 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/apple-watch-series-4-gold/thumbnail.webp'
                WHEN 900003 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/apple-homepod-mini-cosmic-grey/thumbnail.webp'
                WHEN 900004 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/apple-magsafe-battery-pack/thumbnail.webp'
                WHEN 900005 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/tv-studio-camera-pedestal/thumbnail.webp'
                WHEN 900006 THEN N'https://cdn.dummyjson.com/product-images/laptops/apple-macbook-pro-14-inch-space-grey/thumbnail.webp'
                WHEN 900007 THEN N'https://cdn.dummyjson.com/product-images/mens-shirts/gigabyte-aorus-men-tshirt/thumbnail.webp'
                WHEN 900008 THEN N'https://cdn.dummyjson.com/product-images/mens-shirts/blue-&-black-check-shirt/thumbnail.webp'
                WHEN 900009 THEN N'https://cdn.dummyjson.com/product-images/mens-shoes/puma-future-rider-trainers/thumbnail.webp'
                WHEN 900010 THEN N'https://cdn.dummyjson.com/product-images/womens-bags/white-faux-leather-backpack/thumbnail.webp'
                WHEN 900011 THEN N'https://cdn.dummyjson.com/product-images/mens-watches/brown-leather-belt-watch/thumbnail.webp'
                WHEN 900012 THEN N'https://cdn.dummyjson.com/product-images/mens-watches/rolex-datejust/thumbnail.webp'
                WHEN 900013 THEN N'https://cdn.dummyjson.com/product-images/groceries/nescafe-coffee/thumbnail.webp'
                WHEN 900014 THEN N'https://cdn.dummyjson.com/product-images/kitchen-accessories/knife/thumbnail.webp'
                WHEN 900015 THEN N'https://cdn.dummyjson.com/product-images/kitchen-accessories/chopping-board/thumbnail.webp'
                WHEN 900016 THEN N'https://cdn.dummyjson.com/product-images/home-decoration/table-lamp/thumbnail.webp'
                WHEN 900017 THEN N'https://cdn.dummyjson.com/product-images/home-decoration/decoration-swing/thumbnail.webp'
                WHEN 900018 THEN N'https://cdn.dummyjson.com/product-images/home-decoration/house-showpiece-plant/thumbnail.webp'
                WHEN 900019 THEN N'https://cdn.dummyjson.com/product-images/skin-care/vaseline-men-body-and-face-lotion/thumbnail.webp'
                WHEN 900020 THEN N'https://cdn.dummyjson.com/product-images/skin-care/olay-ultra-moisture-shea-butter-body-wash/thumbnail.webp'
                WHEN 900021 THEN N'https://cdn.dummyjson.com/product-images/skin-care/attitude-super-leaves-hand-soap/thumbnail.webp'
                WHEN 900022 THEN N'https://cdn.dummyjson.com/product-images/sports-accessories/tennis-racket/thumbnail.webp'
                WHEN 900023 THEN N'https://cdn.dummyjson.com/product-images/skin-care/vaseline-men-body-and-face-lotion/thumbnail.webp'
                WHEN 900024 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/beats-flex-wireless-earphones/thumbnail.webp'
                WHEN 900025 THEN N'https://cdn.dummyjson.com/product-images/groceries/water/thumbnail.webp'
                WHEN 900026 THEN N'https://cdn.dummyjson.com/product-images/sports-accessories/metal-baseball-bat/thumbnail.webp'
                WHEN 900027 THEN N'https://cdn.dummyjson.com/product-images/womens-bags/white-faux-leather-backpack/thumbnail.webp'
                WHEN 900028 THEN N'https://cdn.dummyjson.com/product-images/home-decoration/table-lamp/thumbnail.webp'
                WHEN 900029 THEN N'https://cdn.dummyjson.com/product-images/sports-accessories/tennis-ball/thumbnail.webp'
                WHEN 900030 THEN N'https://cdn.dummyjson.com/product-images/sports-accessories/feather-shuttlecock/thumbnail.webp'
            END
            WHERE [Id] BETWEEN 900001 AND 900030;

            UPDATE [ProductImages]
            SET [ImageUrl] = CASE [Id]
                WHEN 900001 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/apple-airpods-max-silver/thumbnail.webp'
                WHEN 900002 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/apple-watch-series-4-gold/thumbnail.webp'
                WHEN 900003 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/apple-homepod-mini-cosmic-grey/thumbnail.webp'
                WHEN 900004 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/apple-magsafe-battery-pack/thumbnail.webp'
                WHEN 900005 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/tv-studio-camera-pedestal/thumbnail.webp'
                WHEN 900006 THEN N'https://cdn.dummyjson.com/product-images/laptops/apple-macbook-pro-14-inch-space-grey/thumbnail.webp'
                WHEN 900007 THEN N'https://cdn.dummyjson.com/product-images/mens-shirts/gigabyte-aorus-men-tshirt/thumbnail.webp'
                WHEN 900008 THEN N'https://cdn.dummyjson.com/product-images/mens-shirts/blue-&-black-check-shirt/thumbnail.webp'
                WHEN 900009 THEN N'https://cdn.dummyjson.com/product-images/mens-shoes/puma-future-rider-trainers/thumbnail.webp'
                WHEN 900010 THEN N'https://cdn.dummyjson.com/product-images/womens-bags/white-faux-leather-backpack/thumbnail.webp'
                WHEN 900011 THEN N'https://cdn.dummyjson.com/product-images/mens-watches/brown-leather-belt-watch/thumbnail.webp'
                WHEN 900012 THEN N'https://cdn.dummyjson.com/product-images/mens-watches/rolex-datejust/thumbnail.webp'
                WHEN 900013 THEN N'https://cdn.dummyjson.com/product-images/groceries/nescafe-coffee/thumbnail.webp'
                WHEN 900014 THEN N'https://cdn.dummyjson.com/product-images/kitchen-accessories/knife/thumbnail.webp'
                WHEN 900015 THEN N'https://cdn.dummyjson.com/product-images/kitchen-accessories/chopping-board/thumbnail.webp'
                WHEN 900016 THEN N'https://cdn.dummyjson.com/product-images/home-decoration/table-lamp/thumbnail.webp'
                WHEN 900017 THEN N'https://cdn.dummyjson.com/product-images/home-decoration/decoration-swing/thumbnail.webp'
                WHEN 900018 THEN N'https://cdn.dummyjson.com/product-images/home-decoration/house-showpiece-plant/thumbnail.webp'
                WHEN 900019 THEN N'https://cdn.dummyjson.com/product-images/skin-care/vaseline-men-body-and-face-lotion/thumbnail.webp'
                WHEN 900020 THEN N'https://cdn.dummyjson.com/product-images/skin-care/olay-ultra-moisture-shea-butter-body-wash/thumbnail.webp'
                WHEN 900021 THEN N'https://cdn.dummyjson.com/product-images/skin-care/attitude-super-leaves-hand-soap/thumbnail.webp'
                WHEN 900022 THEN N'https://cdn.dummyjson.com/product-images/sports-accessories/tennis-racket/thumbnail.webp'
                WHEN 900023 THEN N'https://cdn.dummyjson.com/product-images/skin-care/vaseline-men-body-and-face-lotion/thumbnail.webp'
                WHEN 900024 THEN N'https://cdn.dummyjson.com/product-images/mobile-accessories/beats-flex-wireless-earphones/thumbnail.webp'
                WHEN 900025 THEN N'https://cdn.dummyjson.com/product-images/groceries/water/thumbnail.webp'
                WHEN 900026 THEN N'https://cdn.dummyjson.com/product-images/sports-accessories/metal-baseball-bat/thumbnail.webp'
                WHEN 900027 THEN N'https://cdn.dummyjson.com/product-images/womens-bags/white-faux-leather-backpack/thumbnail.webp'
                WHEN 900028 THEN N'https://cdn.dummyjson.com/product-images/home-decoration/table-lamp/thumbnail.webp'
                WHEN 900029 THEN N'https://cdn.dummyjson.com/product-images/sports-accessories/tennis-ball/thumbnail.webp'
                WHEN 900030 THEN N'https://cdn.dummyjson.com/product-images/sports-accessories/feather-shuttlecock/thumbnail.webp'
            END
            WHERE [Id] BETWEEN 900001 AND 900030;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE [Products]
            SET [ImageUrl] = CONCAT(N'https://placehold.co/600x400?text=', REPLACE([Name], N' ', N'+'))
            WHERE [Id] BETWEEN 900001 AND 900030;

            UPDATE [ProductImages]
            SET [ImageUrl] = (
                SELECT [ImageUrl]
                FROM [Products]
                WHERE [Products].[Id] = [ProductImages].[ProductId]
            )
            WHERE [Id] BETWEEN 900001 AND 900030;
            """);
    }
}
