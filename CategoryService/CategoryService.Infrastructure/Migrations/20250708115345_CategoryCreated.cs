using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CategoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CategoryCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CategoryImage = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Category",
                columns: new[] { "Id", "CategoryImage", "Description", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "https://images.pexels.com/photos/18105/pexels-photo.jpg", "Mobile phones, laptops, cameras and more.", "Electronics", null },
                    { 2, "https://images.pexels.com/photos/2983464/pexels-photo-2983464.jpeg", "Men's and Women's apparel for all seasons.", "Clothing", null },
                    { 3, "https://images.pexels.com/photos/276554/pexels-photo-276554.jpeg", "Appliances, furniture, decor and kitchenware.", "Home", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Category");
        }
    }
}
