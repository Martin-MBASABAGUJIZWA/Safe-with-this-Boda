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


            modelBuilder.Entity<Trip>(entity =>
            {
                entity.OwnsOne(t => t.Start);
                entity.OwnsOne(t => t.End);
            });
        }
    }
}
