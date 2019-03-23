using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class UpdateDiscountDataModel : Migration
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

            migrationBuilder.CreateTable(
                name: "DiscountItems",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DiscountId = table.Column<long>(nullable: false),
                    OrderId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscountItems_OrderId",
                table: "DiscountItems",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscountItems");

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
