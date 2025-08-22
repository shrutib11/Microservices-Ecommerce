using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Models;

namespace NotificationService.Infrastructure;

public class NotificationServiceDbContext : DbContext
{
    public NotificationServiceDbContext(DbContextOptions<NotificationServiceDbContext> options) : base(options) { }

    public DbSet<Notifications> Notifications { get; set; }

    public DbSet<UserNotifications> UserNotifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notifications>(entity =>
        {
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.Property(p => p.Type).HasConversion<string>();

            entity.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<UserNotifications>(entity =>
        {
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.Property(p => p.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.Notifications)
            .WithMany()
            .HasForeignKey(d => d.NotificationId)
            .OnDelete(DeleteBehavior.Restrict);

            entity.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        });
    }
}
