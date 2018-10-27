using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class UpdateOrderModelOrderItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ColourId",
                table: "OrderItems",
                column: "ColourId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SizeId",
                table: "OrderItems",
                column: "SizeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Colours_ColourId",
                table: "OrderItems",
                column: "ColourId",
                principalTable: "Colours",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Sizes_SizeId",
                table: "OrderItems",
                column: "SizeId",
                principalTable: "Sizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Colours_ColourId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Sizes_SizeId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ColourId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_SizeId",
                table: "OrderItems");
        }
    }
}
