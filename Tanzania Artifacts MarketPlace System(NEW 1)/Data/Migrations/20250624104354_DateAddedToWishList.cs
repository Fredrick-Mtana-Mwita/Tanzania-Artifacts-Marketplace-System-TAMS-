using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data.Migrations
{
    /// <inheritdoc />
    public partial class DateAddedToWishList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WishListItem_ProductId",
                table: "WishListItem");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAdded",
                table: "WishListItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "WishList",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_WishListItem_ProductId_WishlistId",
                table: "WishListItem",
                columns: new[] { "ProductId", "WishlistId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WishListItem_ProductId_WishlistId",
                table: "WishListItem");

            migrationBuilder.DropColumn(
                name: "DateAdded",
                table: "WishListItem");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "WishList",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_WishListItem_ProductId",
                table: "WishListItem",
                column: "ProductId");
        }
    }
}
