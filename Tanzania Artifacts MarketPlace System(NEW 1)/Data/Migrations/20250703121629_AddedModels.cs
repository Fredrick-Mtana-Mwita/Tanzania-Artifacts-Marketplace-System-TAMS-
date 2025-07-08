using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayPalPaymentId",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerEmail",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayPalPaymentId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "PayerEmail",
                table: "Payment");
        }
    }
}
