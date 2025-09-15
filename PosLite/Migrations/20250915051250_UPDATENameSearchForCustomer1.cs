using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosLite.Migrations
{
    /// <inheritdoc />
    public partial class UPDATENameSearchForCustomer1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Customers_CodeSearch",
                table: "Customers",
                column: "CodeSearch");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NameSearch",
                table: "Customers",
                column: "NameSearch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_CodeSearch",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_NameSearch",
                table: "Customers");
        }
    }
}
