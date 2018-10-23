using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class UpdateOrderModelEditReturn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Returns_ReturnId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_ReturnId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "Returns");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnItems_ReturnId",
                table: "ReturnItems",
                column: "ReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnItems_Returns_ReturnId",
                table: "ReturnItems",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnItems_Returns_ReturnId",
                table: "ReturnItems");

            migrationBuilder.DropIndex(
                name: "IX_ReturnItems_ReturnId",
                table: "ReturnItems");

            migrationBuilder.AddColumn<long>(
                name: "ReturnId",
                table: "Returns",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Returns_ReturnId",
                table: "Returns",
                column: "ReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Returns_ReturnId",
                table: "Returns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
