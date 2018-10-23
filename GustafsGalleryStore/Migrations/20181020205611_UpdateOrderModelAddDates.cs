using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GustafsGalleryStore.Migrations
{
    public partial class UpdateOrderModelAddDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RefundCreatedDate",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnReceivedDate",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnRequestedDate",
                table: "Orders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundCreatedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnReceivedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReturnRequestedDate",
                table: "Orders");
        }
    }
}
