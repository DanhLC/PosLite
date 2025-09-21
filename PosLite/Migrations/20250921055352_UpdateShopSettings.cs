using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosLite.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShopSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "ShopSettings",
                newName: "JsonValue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JsonValue",
                table: "ShopSettings",
                newName: "Value");
        }
    }
}
