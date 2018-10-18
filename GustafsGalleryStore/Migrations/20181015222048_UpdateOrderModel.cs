using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class UpdateOrderModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHistories_ProductBrands_OrderStatusId",
                table: "OrderHistories");

            migrationBuilder.AddColumn<string>(
                name: "PaymentId",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMessage",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerMessage",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHistories_OrderStatuses_OrderStatusId",
                table: "OrderHistories",
                column: "OrderStatusId",
                principalTable: "OrderStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHistories_OrderStatuses_OrderStatusId",
                table: "OrderHistories");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentMessage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SellerMessage",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHistories_ProductBrands_OrderStatusId",
                table: "OrderHistories",
                column: "OrderStatusId",
                principalTable: "ProductBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
