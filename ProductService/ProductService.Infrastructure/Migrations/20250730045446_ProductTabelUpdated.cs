using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductTabelUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AvgRating",
                table: "Products",
                type: "numeric(3,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvgRating", "CreatedAt", "TotalReviews" },
                values: new object[] { null, new DateTime(2025, 7, 30, 10, 24, 46, 537, DateTimeKind.Local).AddTicks(2098), null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AvgRating", "CreatedAt", "TotalReviews" },
                values: new object[] { null, new DateTime(2025, 7, 30, 10, 24, 46, 537, DateTimeKind.Local).AddTicks(2102), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgRating",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 8, 11, 34, 5, 552, DateTimeKind.Local).AddTicks(8981));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 8, 11, 34, 5, 552, DateTimeKind.Local).AddTicks(8992));
        }
    }
}
