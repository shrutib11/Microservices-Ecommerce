using Microsoft.EntityFrameworkCore;
using SupportService.Domain.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace SupportService.Infrastructure;

public class SupportServiceDbContext : DbContext
{
    public SupportServiceDbContext(DbContextOptions<SupportServiceDbContext> options) : base(options) { }

    public DbSet<FAQs> FAQs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var converter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            );

        //To detect the changes in keyword list
        var comparer = new ValueComparer<List<string>>(
            (l1, l2) => l1!.SequenceEqual(l2!), 
            l => l.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), 
            l => l.ToList()
        );

        modelBuilder.Entity<FAQs>(entity =>
        {
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn();

            entity.Property(f => f.Keyword)
                .HasConversion(converter)
                .HasColumnType("jsonb")
                .Metadata.SetValueComparer(comparer);
        });
    }
}
