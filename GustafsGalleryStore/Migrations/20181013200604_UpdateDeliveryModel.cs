using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class UpdateDeliveryModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryType_DeliveryCompany_DeliveryCompanyId",
                table: "DeliveryType");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliveryType_DeliveryTypeId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeliveryType",
                table: "DeliveryType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeliveryCompany",
                table: "DeliveryCompany");

            migrationBuilder.RenameTable(
                name: "DeliveryType",
                newName: "DeliveryTypes");

            migrationBuilder.RenameTable(
                name: "DeliveryCompany",
                newName: "DeliveryCompanies");

            migrationBuilder.RenameIndex(
                name: "IX_DeliveryType_DeliveryCompanyId",
                table: "DeliveryTypes",
                newName: "IX_DeliveryTypes_DeliveryCompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeliveryTypes",
                table: "DeliveryTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeliveryCompanies",
                table: "DeliveryCompanies",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryTypes_DeliveryCompanies_DeliveryCompanyId",
                table: "DeliveryTypes",
                column: "DeliveryCompanyId",
                principalTable: "DeliveryCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliveryTypes_DeliveryTypeId",
                table: "Orders",
                column: "DeliveryTypeId",
                principalTable: "DeliveryTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryTypes_DeliveryCompanies_DeliveryCompanyId",
                table: "DeliveryTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliveryTypes_DeliveryTypeId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeliveryTypes",
                table: "DeliveryTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeliveryCompanies",
                table: "DeliveryCompanies");

            migrationBuilder.RenameTable(
                name: "DeliveryTypes",
                newName: "DeliveryType");

            migrationBuilder.RenameTable(
                name: "DeliveryCompanies",
                newName: "DeliveryCompany");

            migrationBuilder.RenameIndex(
                name: "IX_DeliveryTypes_DeliveryCompanyId",
                table: "DeliveryType",
                newName: "IX_DeliveryType_DeliveryCompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeliveryType",
                table: "DeliveryType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeliveryCompany",
                table: "DeliveryCompany",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryType_DeliveryCompany_DeliveryCompanyId",
                table: "DeliveryType",
                column: "DeliveryCompanyId",
                principalTable: "DeliveryCompany",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliveryType_DeliveryTypeId",
                table: "Orders",
                column: "DeliveryTypeId",
                principalTable: "DeliveryType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
