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
        public DbSet<ParkedVehicle> ParkedVehicles 
        {
            get; set;
        }

}
}
