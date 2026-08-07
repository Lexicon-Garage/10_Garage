using Garage.Web.Data;
using Garage.Web.Services;
using Garage.Web.ViewModels;
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
            builder.Services.AddDbContext<AppDbConext>(options =>
            options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
			builder.Services.AddScoped<IFileHandler<Stream, ReceiptViewModel>, PdfFileHandler>();
				
			var app = builder.Build();
			
			using (var scope = app.Services.CreateScope())
			{
    			var context = scope.ServiceProvider.GetRequiredService<AppDbConext>();
    			DbInitializer.Seed(context);
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

			app.UseAuthorization();

			app.MapStaticAssets();
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=ParkedVehicles}/{action=Index}/{id?}")
				.WithStaticAssets();

			app.Run();
		}
	}
}
