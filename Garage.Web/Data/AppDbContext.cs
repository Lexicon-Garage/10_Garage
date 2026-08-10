using Garage.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Garage.Web.Data
{
    public class AppDbConext : DbContext
    {
        public AppDbConext(DbContextOptions<AppDbConext> options)
        : base(options)
        {
        }
        public DbSet<Vehicle> Vehicles
        {
            get; set;
        }
        public DbSet<VehicleType> VehicleTypes
        {
            get; set;
        }
        public DbSet<BrandType> BrandTypes
        {
            get; set;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParkingAllocation>()
                .HasOne(pa => pa.ParkingSpot)
                .WithMany(ps => ps.ParkingAllocations)
                .HasForeignKey(pa => pa.ParkingSpotId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ParkingAllocation>()
            .HasOne(x => x.ParkingSession)
            .WithMany(x => x.ParkingAllocations)
            .HasForeignKey(x => x.ParkingSessionId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
