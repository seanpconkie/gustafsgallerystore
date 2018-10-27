using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class UpdateOrderModelForPayPal2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayPalCartId",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayPalPayerId",
                table: "Orders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayPalCartId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PayPalPayerId",
                table: "Orders");
        }
    }
}
