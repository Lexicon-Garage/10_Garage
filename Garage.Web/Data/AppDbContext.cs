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
    }
}
