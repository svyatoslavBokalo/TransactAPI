using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionAPI.Migrations
{
    /// <inheritdoc />
    public partial class changeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Transactions",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Transactions",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Transactions",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "TransactionDate",
                table: "Transactions",
                newName: "transaction_date");

            migrationBuilder.RenameColumn(
                name: "ClientLocation",
                table: "Transactions",
                newName: "client_location");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Transactions",
                newName: "transaction_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "Transactions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Transactions",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "Transactions",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "transaction_date",
                table: "Transactions",
                newName: "TransactionDate");

            migrationBuilder.RenameColumn(
                name: "client_location",
                table: "Transactions",
                newName: "ClientLocation");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                table: "Transactions",
                newName: "TransactionId");
        }
    }
}
