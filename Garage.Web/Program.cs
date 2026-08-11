using Garage.Web.Data;
using Garage.Web.Services;
using Garage.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Garage.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace Garage.Web
{
	public class Program
	{
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            QuestPDF.Settings.License = LicenseType.Community;

            // Add services to the container.
            builder.Services.AddControllersWithViews();
         
            builder.Services.AddDbContext<AppDbConext>(options =>
             options.UseSqlServer(
             builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services
                .AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbConext>();

            builder.Services.AddScoped<IFileHandler<Stream, ReceiptViewModel>, PdfFileHandler>();
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var context = services.GetRequiredService<AppDbConext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await DbInitializer.SeedAsync(
                    context,
                    userManager,
                    roleManager);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseRouting();
			
			app.UseAuthentication();
			app.UseAuthorization();

			app.MapStaticAssets();
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Vehicles}/{action=Index}/{id?}")
				.WithStaticAssets();
			app.MapRazorPages();
			app.Run();
		}
	}
}
