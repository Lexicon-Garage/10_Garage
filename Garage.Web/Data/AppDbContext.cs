using Garage.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Garage.Web.Data

{
    public class AppDbConext : IdentityDbContext
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
