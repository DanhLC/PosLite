using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosLite.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryNameSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameSearch",
                table: "Categories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NameSearch",
                table: "Categories",
                column: "NameSearch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_NameSearch",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NameSearch",
                table: "Categories");
        }
    }
}
