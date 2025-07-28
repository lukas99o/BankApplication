using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApplication.Migrations
{
    /// <inheritdoc />
    public partial class BankAccountUpgrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "BankAccounts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "BankAccounts");
        }
    }
}
