using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddNullablePaymentCardToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentCardId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentCards",
                columns: table => new
                {
                    CardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ExpirationDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CVV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillingAddressLine1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingAddressLine2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BillingState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BillingPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BillingCountry = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentCards", x => x.CardId);
                    table.ForeignKey(
                        name: "FK_PaymentCards_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentCardId",
                table: "Orders",
                column: "PaymentCardId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCards_UserId",
                table: "PaymentCards",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PaymentCards_PaymentCardId",
                table: "Orders",
                column: "PaymentCardId",
                principalTable: "PaymentCards",
                principalColumn: "CardId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PaymentCards_PaymentCardId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "PaymentCards");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymentCardId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentCardId",
                table: "Orders");
        }
    }
}
