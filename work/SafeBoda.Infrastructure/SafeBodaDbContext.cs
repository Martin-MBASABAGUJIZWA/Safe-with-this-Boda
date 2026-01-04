// In SafeBodaDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SafeBoda.Core;

namespace SafeBoda.Infrastructure
{
    public class SafeBodaDbContext : IdentityDbContext<ApplicationUser>
    {
        public SafeBodaDbContext(DbContextOptions<SafeBodaDbContext> options)
            : base(options) { }

        public DbSet<Rider> Riders { get; set; } = null!;
        public DbSet<Driver> Drivers { get; set; } = null!;
        public DbSet<Trip> Trips { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Remove any collation specifications for SQLite compatibility
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        property.SetCollation(null);
                    }
                }
            }

            // Seed Riders
            modelBuilder.Entity<Rider>().HasData(
                new Rider(
                    Id: Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name: "Test Rider",
                    PhoneNumber: "1234567890"
                )
            );

            // Seed Drivers
            modelBuilder.Entity<Driver>().HasData(
                new Driver(
                    Id: Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name: "Test Driver",
                    PhoneNumber: "0987654321",
                    MotoPlateNumber: "RAA123A"
                )
            );

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.OwnsOne(t => t.Start).HasData(
                    new { TripId = Guid.Parse("11111111-1111-1111-1111-111111111111"), Latitude = -1.94995, Longitude = 30.05885 }
                );

                entity.OwnsOne(t => t.End).HasData(
                    new { TripId = Guid.Parse("11111111-1111-1111-1111-111111111111"), Latitude = -1.95765, Longitude = 30.09123 }
                );

                entity.HasData(
                    new Trip
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        RiderId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        DriverId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        Fare = 1500m,
                        RequestTime = DateTime.SpecifyKind(new DateTime(2024, 1, 1, 0, 0, 0), DateTimeKind.Utc)
                    }
                );
            });
        }
    }
}