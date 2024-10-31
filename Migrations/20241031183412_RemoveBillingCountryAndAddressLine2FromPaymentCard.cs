using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBillingCountryAndAddressLine2FromPaymentCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingAddressLine2",
                table: "PaymentCards");

            migrationBuilder.DropColumn(
                name: "BillingCountry",
                table: "PaymentCards");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingAddressLine2",
                table: "PaymentCards",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BillingCountry",
                table: "PaymentCards",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
