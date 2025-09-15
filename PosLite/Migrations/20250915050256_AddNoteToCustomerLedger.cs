using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosLite.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteToCustomerLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Qty",
                table: "SaleInvoiceLines");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "CustomerProductDiscounts");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "CustomerProductDiscounts");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "SaleInvoiceLines",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "LineTotal",
                table: "SaleInvoiceLines",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "SaleInvoiceLines",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "SaleInvoiceLines",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "SaleInvoiceLines",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "SaleInvoiceLines",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "CustomerLedgers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoiceLines_ProductId",
                table: "SaleInvoiceLines",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleInvoiceLines_Products_ProductId",
                table: "SaleInvoiceLines",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoiceLines_Products_ProductId",
                table: "SaleInvoiceLines");

            migrationBuilder.DropIndex(
                name: "IX_SaleInvoiceLines_ProductId",
                table: "SaleInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "SaleInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "SaleInvoiceLines");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "SaleInvoiceLines");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "CustomerLedgers");

            migrationBuilder.AlterColumn<int>(
                name: "UnitPrice",
                table: "SaleInvoiceLines",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "LineTotal",
                table: "SaleInvoiceLines",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "DiscountAmount",
                table: "SaleInvoiceLines",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AddColumn<double>(
                name: "Qty",
                table: "SaleInvoiceLines",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "CustomerProductDiscounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "CustomerProductDiscounts",
                type: "TEXT",
                nullable: true);
        }
    }
}
