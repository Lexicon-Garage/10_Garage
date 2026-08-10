using Garage.Web.Data;
using Garage.Web.Models;
using Garage.Web.Services;
using Garage.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

namespace Garage.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
            var builder = WebApplication.CreateBuilder(args);

            QuestPDF.Settings.License = LicenseType.Community;


			// Add services to the container.			
			builder.Services.AddControllersWithViews();
			builder.Services.AddRazorPages();

			builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlServer(
			builder.Configuration.GetConnectionString("DefaultConnection")));
									
			builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
			{
				options.SignIn.RequireConfirmedAccount = false;
				options.Password.RequireDigit = false;
				options.Password.RequiredLength = 4;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
			})
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<AppDbContext>();



			builder.Services.AddScoped<IFileHandler<Stream, ReceiptViewModel>, PdfFileHandler>();
            var app = builder.Build();

			using (var scope = app.Services.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
				db.Database.Migrate();
				//DbInitializer.Seed(db);
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
				pattern: "{controller=ParkedVehicles}/{action=Index}/{id?}")
				.WithStaticAssets();

			app.MapRazorPages();

			app.Run();
		}
	}
}
