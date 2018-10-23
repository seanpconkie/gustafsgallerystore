using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class UpdateOrderModel2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefundId",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundMessage",
                table: "Orders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RefundMessage",
                table: "Orders");
        }
    }
}
