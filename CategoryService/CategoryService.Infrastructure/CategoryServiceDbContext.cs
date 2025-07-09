using CategoryService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CategoryService.Infrastructure;

public class CategoryServiceDbContext : DbContext
{
    public CategoryServiceDbContext(DbContextOptions<CategoryServiceDbContext> options) : base(options) { }

    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.CategoryImage)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone");

            entity.HasData(
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Mobile phones, laptops, cameras and more.",
                    CategoryImage = "https://images.pexels.com/photos/18105/pexels-photo.jpg",
                },
                new Category
                {
                    Id = 2,
                    Name = "Clothing",
                    Description = "Men's and Women's apparel for all seasons.",
                    CategoryImage = "https://images.pexels.com/photos/2983464/pexels-photo-2983464.jpeg",
                },
                new Category
                {
                    Id = 3,
                    Name = "Home",
                    Description = "Appliances, furniture, decor and kitchenware.",
                    CategoryImage = "https://images.pexels.com/photos/276554/pexels-photo-276554.jpeg",
                }
            );
        });
    }
}
