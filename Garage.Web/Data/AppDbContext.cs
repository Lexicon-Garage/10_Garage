using Garage.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Garage.Web.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
        public DbSet<BrandType> BrandTypes => Set<BrandType>();
        public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();
        public DbSet<ParkingSession> ParkingSessions => Set<ParkingSession>();
        public DbSet<ParkingAllocation> ParkingAllocations => Set<ParkingAllocation>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);   // required — creates the Identity tables             

            builder.Entity<ParkingSession>()
                .HasIndex(s => s.VehicleId)
                .IsUnique()
                .HasFilter("[CheckOutTime] IS NULL");

            builder.Entity<ParkingSession>()
                .Property(s => s.HourlyRateAtCheckIn).HasPrecision(18, 2);
            builder.Entity<ParkingSession>()
                .Property(s => s.TotalPrice).HasPrecision(18, 2);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Owner).WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ParkingSession>()
                .HasOne(s => s.Vehicle).WithMany(v => v!.ParkingSessions)
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ParkingAllocation>()
                .HasOne(a => a.ParkingSpot).WithMany(p => p!.ParkingAllocations)
                .HasForeignKey(a => a.ParkingSpotId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 