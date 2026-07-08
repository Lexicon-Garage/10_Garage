using Garage.Web;
using Garage.Web.Data;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Garage.Test.IntegrationTest
{
    public class GarageWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var antiforgery = services.SingleOrDefault(
                    x => x.ServiceType == typeof(IAntiforgery));

                if (antiforgery != null)
                {
                    services.Remove(antiforgery);
                }

                services.AddSingleton<IAntiforgery, NoopAntiforgery>();
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<AppDbConext>();

            context.Database.EnsureCreated();

            TestDataSeeder.Seed(context);

            return host;
        }
    }
}
