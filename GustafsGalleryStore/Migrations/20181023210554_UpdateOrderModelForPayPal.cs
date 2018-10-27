using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class UpdateOrderModelForPayPal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayPalPaymentId",
                table: "Orders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayPalPaymentId",
                table: "Orders");
        }
    }
}
