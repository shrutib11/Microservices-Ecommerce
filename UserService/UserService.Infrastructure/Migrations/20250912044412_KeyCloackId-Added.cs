using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class KeyCloackIdAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeycloackId",
                table: "Users",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "KeycloackId" },
                values: new object[] { new DateTime(2025, 9, 12, 10, 14, 12, 213, DateTimeKind.Local).AddTicks(8187), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "KeycloackId" },
                values: new object[] { new DateTime(2025, 9, 12, 10, 14, 12, 213, DateTimeKind.Local).AddTicks(8191), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "KeycloackId" },
                values: new object[] { new DateTime(2025, 9, 12, 10, 14, 12, 213, DateTimeKind.Local).AddTicks(8193), "" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KeycloackId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 17, 13, 42, 18, 645, DateTimeKind.Local).AddTicks(5967));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 17, 13, 42, 18, 645, DateTimeKind.Local).AddTicks(5970));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 17, 13, 42, 18, 645, DateTimeKind.Local).AddTicks(5973));
        }
    }
}
