using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosLite.Migrations
{
    /// <inheritdoc />
    public partial class UPDATENameSearchForCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeSearch",
                table: "Customers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameSearch",
                table: "Customers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeSearch",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NameSearch",
                table: "Customers");
        }
    }
}
