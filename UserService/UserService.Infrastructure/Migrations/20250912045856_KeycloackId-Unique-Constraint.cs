using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class KeycloackIdUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "KeycloackId",
                table: "Users",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(36)",
                oldMaxLength: 36,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 10, 28, 56, 565, DateTimeKind.Local).AddTicks(8046));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 10, 28, 56, 565, DateTimeKind.Local).AddTicks(8051));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 10, 28, 56, 565, DateTimeKind.Local).AddTicks(8053));

            migrationBuilder.CreateIndex(
                name: "IX_Users_KeycloackId",
                table: "Users",
                column: "KeycloackId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_KeycloackId",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "KeycloackId",
                table: "Users",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(36)",
                oldMaxLength: 36);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 10, 14, 12, 213, DateTimeKind.Local).AddTicks(8187));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 10, 14, 12, 213, DateTimeKind.Local).AddTicks(8191));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 10, 14, 12, 213, DateTimeKind.Local).AddTicks(8193));
        }
    }
}
