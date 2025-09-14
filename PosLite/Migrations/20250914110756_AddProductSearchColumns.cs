using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosLite.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSearchColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeSearch",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameSearch",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CodeSearch",
                table: "Products",
                column: "CodeSearch");

            migrationBuilder.CreateIndex(
                name: "IX_Products_NameSearch",
                table: "Products",
                column: "NameSearch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CodeSearch",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_NameSearch",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CodeSearch",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NameSearch",
                table: "Products");
        }
    }
}
