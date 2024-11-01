using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class RenameCardHolderNameToName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CardHolderName",
                table: "PaymentCards",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "PaymentCards",
                newName: "CardHolderName");
        }
    }
}
