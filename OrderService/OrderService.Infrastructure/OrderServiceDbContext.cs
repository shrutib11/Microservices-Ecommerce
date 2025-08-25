using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Models;

namespace OrderService.Infrastructure;

public class OrderServiceDbContext : DbContext
{
    public OrderServiceDbContext(DbContextOptions<OrderServiceDbContext> options) : base(options)
    {
    }

    public DbSet<OrderEvent> OrderEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderEvent>(entity =>
        {
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn();

            entity.Property(p => p.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()");

            entity.Property(p => p.UpdatedAt)
                .HasColumnType("timestamp without time zone");

            entity.Property(p => p.OrderId)
                .HasDefaultValueSql("concat('ORD-', substr(md5(random()::text), 1, 6))")
                .IsRequired();

            entity.Property(p => p.UserId)
                .IsRequired();

            entity.Property(p => p.Status)
                .HasConversion<string>()
                .IsRequired();

        });
    }


}
