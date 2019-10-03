using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class RemoveOrderIDFromDiscounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discounts_Orders_OrderId",
                table: "Discounts");

            migrationBuilder.DropIndex(
                name: "IX_Discounts_OrderId",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Discounts");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountItems_DiscountId",
                table: "DiscountItems",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountItems_OrderId",
                table: "DiscountItems",
                column: "OrderId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_DiscountItems_Discounts_DiscountId",
            //    table: "DiscountItems",
            //    column: "DiscountId",
            //    principalTable: "Discounts",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_DiscountItems_Orders_OrderId",
            //    table: "DiscountItems",
            //    column: "OrderId",
            //    principalTable: "Orders",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_DiscountItems_Discounts_DiscountId",
            //    table: "DiscountItems");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_DiscountItems_Orders_OrderId",
            //    table: "DiscountItems");

            migrationBuilder.DropIndex(
                name: "IX_DiscountItems_DiscountId",
                table: "DiscountItems");

            migrationBuilder.DropIndex(
                name: "IX_DiscountItems_OrderId",
                table: "DiscountItems");

            migrationBuilder.AddColumn<long>(
                name: "OrderId",
                table: "Discounts",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_OrderId",
                table: "Discounts",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Discounts_Orders_OrderId",
                table: "Discounts",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
