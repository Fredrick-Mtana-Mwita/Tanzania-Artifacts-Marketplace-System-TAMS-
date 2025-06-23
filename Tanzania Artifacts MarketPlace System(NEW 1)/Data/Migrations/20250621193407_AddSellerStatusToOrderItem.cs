using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerStatusToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AltText",
                table: "ProductImage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductImage",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "ProductImage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "ProductImage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SellerStatus",
                table: "OrderItem",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AltText",
                table: "ProductImage");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProductImage");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "ProductImage");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "ProductImage");

            migrationBuilder.DropColumn(
                name: "SellerStatus",
                table: "OrderItem");
        }
    }
}
