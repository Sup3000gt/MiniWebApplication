using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToPaymentCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PaymentCards",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PaymentCards");
        }
    }
}
