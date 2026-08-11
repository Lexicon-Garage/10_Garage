using Garage.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Garage.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            AppDbConext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Apply pending migrations
            await context.Database.MigrateAsync();

            // -------------------------
            // Seed role
            // -------------------------

            const string adminRole = "Admin";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                var role = new IdentityRole(adminRole);

                var roleResult = await roleManager.CreateAsync(role);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new Exception(
                        $"Failed to create role: {errors}");
                }
            }

            // -------------------------
            // Seed user
            // -------------------------

            const string email = "admin@garage.se";
            const string password = "Admin123!";

            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,

                    FirstName = "Admin",
                    LastName = "Garage",

                    PersonalNumber = "19900101-1234",

                    ProMembershipStart = DateTime.Now,
                    ProMembershipEnd = DateTime.Now.AddYears(1)
                };

                var result = await userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description));

                    throw new Exception(
                        $"Failed to create seed user: {errors}");
                }

                // Add user to Admin role
                var roleResult = await userManager.AddToRoleAsync(
                    user,
                    adminRole);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new Exception(
                        $"Failed to assign role: {errors}");
                }
            }

            // -------------------------
            // Seed vehicles
            // -------------------------

            if (context.Vehicles.Any())
                return;

            var now = DateTime.Now;

            // Add vehicles here if needed

            await context.SaveChangesAsync();
        }
    }
}