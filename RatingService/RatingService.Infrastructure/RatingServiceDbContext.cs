using Microsoft.EntityFrameworkCore;
using RatingService.Domain.Models;

namespace RatingService.Infrastructure;

public class RatingServiceDbContext : DbContext
{
    public RatingServiceDbContext(DbContextOptions<RatingServiceDbContext> options) : base(options) { }

    public DbSet<Rating> Ratings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.RatingValue)
                 .IsRequired()
                 .HasDefaultValue(1);

            entity.Property(r => r.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()");

            entity.Property(r => r.Comment)
                .HasMaxLength(2000);

            entity.HasIndex(e => new { e.OrderId, e.UserId, e.ProductId })
                .IsUnique();
        });
    }
}
