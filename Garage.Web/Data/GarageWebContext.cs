using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class GarageWebContext(DbContextOptions<GarageWebContext> options) : IdentityDbContext<Garage.Web.Data.ApplicationUser>(options)
{
}
