using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class UpdateOrderModelAddPayPalSaleId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayPalSaleId",
                table: "Orders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayPalSaleId",
                table: "Orders");
        }
    }
}
