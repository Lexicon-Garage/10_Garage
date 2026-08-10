using Garage.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Garage.Web.Data
{
    public class AppDbConext : IdentityDbContext<ApplicationUser>
    {
        public AppDbConext(DbContextOptions<AppDbConext> options)
        : base(options)
        {
        }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleType> VehicleTypes
        {
            get; set;
        }
        public DbSet<BrandType> BrandTypes
        {
            get; set;
        }
        public DbSet<ParkingSession> ParkingSessions { get; set; }
        public DbSet<ParkingSpot> ParkingSpots { get; set; }
        public DbSet<ParkingAllocation> ParkingAllocations { get; set; }
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
