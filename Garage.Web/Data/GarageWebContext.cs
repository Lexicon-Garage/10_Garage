using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class GarageWebContext(DbContextOptions<GarageWebContext> options) : IdentityDbContext<Garage.Web.Models.ApplicationUser>(options)
{
}
