using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    PinCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    ProfileImage = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "FirstName", "LastName", "Password", "PhoneNumber", "PinCode", "ProfileImage", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Amreli, India", new DateTime(2025, 7, 8, 11, 43, 31, 430, DateTimeKind.Local).AddTicks(9853), "dhruvilchotaliya@gmail.com", "Dhruvil", "Chotaliya", "Dhruvil@123", "1234567890", "365601", null, null },
                    { 2, "Ahmedabad, India", new DateTime(2025, 7, 8, 11, 43, 31, 430, DateTimeKind.Local).AddTicks(9855), "shrutibhalodia@gmail.com", "Shruti", "Bhalodia", "Shruti@123", "1234567890", "380001", null, null },
                    { 3, "Ahmedabad, India", new DateTime(2025, 7, 8, 11, 43, 31, 430, DateTimeKind.Local).AddTicks(9858), "richakamani@gmail.com", "Richa", "Kamani", "Richa@123", "1234567890", "380001", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
