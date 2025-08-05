using Microsoft.EntityFrameworkCore;
using ProductService.Domain;
using ProductService.Domain.Models;

namespace ProductService.Infrastructure;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }

    public DbSet<ProductMedia> ProductMedias { get; set; }

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

            entity.Property(e => e.AvgRating).HasColumnType("decimal(3,2)");

        });

        modelBuilder.Entity<ProductMedia>(entity =>
        {
            entity.Property(p => p.MediaType)
            .HasConversion<string>();

            entity.Property(p => p.DisplayOrder)
            .IsRequired();

            entity.Property(p => p.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .HasDefaultValueSql("NOW()");

            entity.Property(p => p.UpdatedAt)
            .HasColumnType("timestamp without time zone");

            entity.Property(p => p.IsDeleted)
            .HasDefaultValue(false);
            
            entity
            .HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
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
               IsDeleted = false,
               CreatedAt = DateTime.Now,
               UpdatedAt = null
           }

       );
    }
}


