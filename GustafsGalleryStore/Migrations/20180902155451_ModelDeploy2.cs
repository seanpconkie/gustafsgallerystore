using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class ModelDeploy2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProductId",
                table: "ProductSizes",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProductId",
                table: "ProductColours",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSizes_ProductId",
                table: "ProductSizes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductColours_ProductId",
                table: "ProductColours",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductColours_Products_ProductId",
                table: "ProductColours",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSizes_Products_ProductId",
                table: "ProductSizes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductColours_Products_ProductId",
                table: "ProductColours");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSizes_Products_ProductId",
                table: "ProductSizes");

            migrationBuilder.DropIndex(
                name: "IX_ProductSizes_ProductId",
                table: "ProductSizes");

            migrationBuilder.DropIndex(
                name: "IX_ProductColours_ProductId",
                table: "ProductColours");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ProductSizes");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ProductColours");
        }
    }
}
