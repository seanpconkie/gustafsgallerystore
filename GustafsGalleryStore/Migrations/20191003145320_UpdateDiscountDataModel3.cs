using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class UpdateDiscountDataModel3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasMaxUse",
                table: "Discounts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasMinValue",
                table: "Discounts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MinSpend",
                table: "Discounts",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasMaxUse",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "HasMinValue",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "MinSpend",
                table: "Discounts");
        }
    }
}
