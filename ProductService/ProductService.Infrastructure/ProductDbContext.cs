using Microsoft.EntityFrameworkCore;
using ProductService.Domain;

namespace ProductService.Infrastructure;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn();

            entity.Property(p => p.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()");

            entity.Property(p => p.UpdatedAt)
                .HasColumnType("timestamp without time zone");

            entity.Property(p => p.IsDeleted)
                .HasDefaultValue(false);

        });

        modelBuilder.Entity<Product>().HasData(
           new Product
           {
               Id = 1,
               Name = "Sample Product 1",
               Description = "This is a sample product.",
               Price = 99.99M,
               StockQuantity = 10,
               CategoryId = 1,
               ProductImage = "sample1.jpg",
               IsDeleted = false,
               CreatedAt = DateTime.Now,
               UpdatedAt = null
           },
           new Product
           {
               Id = 2,
               Name = "Sample Product 2",
               Description = "Another sample product.",
               Price = 49.99M,
               StockQuantity = 5,
               CategoryId = 2,
               ProductImage = "sample2.jpg",
               IsDeleted = false,
               CreatedAt = DateTime.Now,
               UpdatedAt = null
           }
       
       );
    }
}


