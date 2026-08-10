using Garage.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Garage.Web.Data
{
	public class AppDbContext : IdentityDbContext<ApplicationUser>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)
			: base(options)
		{
		}

		public DbSet<VehicleType> VehicleTypes { get; set; }

		public DbSet<BrandType> BrandTypes { get; set; }

		public DbSet<Vehicle> Vehicles { get; set; }

		public DbSet<ParkingSpot> ParkingSpots { get; set; }

		public DbSet<ParkingSession> ParkingSessions { get; set; }

		public DbSet<ParkingAllocation> ParkingAllocations { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
		}
	}
}
