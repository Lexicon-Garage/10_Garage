using Garage.Web.Models;

namespace Garage.Web.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.VehicleTypes.Any())
            {
                context.VehicleTypes.AddRange(
                    new VehicleType { Name = "Motorcycle" },
                    new VehicleType { Name = "Car" },
                    new VehicleType { Name = "Bus" },
                    new VehicleType { Name = "Truck" },
                    new VehicleType { Name = "Boat" });
            }

            if (!context.BrandTypes.Any())
            {
                context.BrandTypes.AddRange(
                    new BrandType { Name = "Toyota" },
                    new BrandType { Name = "Volvo" },
                    new BrandType { Name = "Ford" },
                    new BrandType { Name = "Volkswagen" },
                    new BrandType { Name = "BMW" });
            }

            context.SaveChanges();
        }
    }
}