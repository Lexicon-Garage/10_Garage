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
        public DbSet<ParkedVehicle> ParkedVehicles { get; set; }
        public DbSet<ParkingSpot> ParkingSpots { get; set; }
        public DbSet<ParkingAllocation> ParkingAllocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParkingAllocation>()
           .HasOne(x => x.ParkingSpot)
           .WithMany(x => x.ParkingAllocations)
           .HasForeignKey(x => x.ParkingSpotId);

            modelBuilder.Entity<ParkingAllocation>()
                .HasOne(x => x.ParkedVehicle)
                .WithMany(x => x.ParkingAllocations)
                .HasForeignKey(x => x.ParkedVehicleId);
        }
    }
}
