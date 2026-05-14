using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestAccessTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestAccessToken",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "Carts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestAccessToken",
                table: "Carts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GuestAccessToken",
                table: "Orders",
                column: "GuestAccessToken",
                unique: true,
                filter: "[GuestAccessToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_GuestAccessToken",
                table: "Carts",
                column: "GuestAccessToken",
                unique: true,
                filter: "[GuestAccessToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_SessionId",
                table: "Carts",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_GuestAccessToken",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Carts_GuestAccessToken",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_SessionId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "GuestAccessToken",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestAccessToken",
                table: "Carts");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
