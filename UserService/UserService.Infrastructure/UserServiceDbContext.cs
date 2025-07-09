
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Models;

namespace UserService.Infrastructure
{
    public class UserServiceDbContext : DbContext
    {
        public UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(e => e.Id).UseIdentityAlwaysColumn();

            modelBuilder.Entity<User>().Property(p => p.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<User>().Property(p => p.UpdatedAt)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "dhruvilchotaliya@gmail.com",
                Password = "Dhruvil@123",
                FirstName = "Dhruvil",
                LastName = "Chotaliya",
                PhoneNumber = "1234567890",
                PinCode = "365601",
                Address = "Amreli, India",
                ProfileImage = null,
                CreatedAt = DateTime.Now
            },
            new User
            {
                Id = 2,
                Email = "shrutibhalodia@gmail.com",
                Password = "Shruti@123",
                FirstName = "Shruti",
                LastName = "Bhalodia",
                PhoneNumber = "1234567890",
                PinCode = "380001",
                Address = "Ahmedabad, India",
                ProfileImage = null,
                CreatedAt = DateTime.Now
            },
            new User
            {
                Id = 3,
                Email = "richakamani@gmail.com",
                Password = "Richa@123",
                FirstName = "Richa",
                LastName = "Kamani",
                PhoneNumber = "1234567890",
                PinCode = "380001",
                Address = "Ahmedabad, India",
                ProfileImage = null,
                CreatedAt = DateTime.Now
            }
    );
        }
    }
}