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
        public DbSet<ParkedVehicle> ParkedVehicles 
        {
            get; set;
        }

}
}
