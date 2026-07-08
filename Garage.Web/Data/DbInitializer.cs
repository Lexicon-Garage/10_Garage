using Garage.Web.Models;

namespace Garage.Web.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbConext context)
        {
            if (context.ParkedVehicles.Any())
                return; // never overwrite real data

            var now = DateTime.Now;

            var vehicles = new List<ParkedVehicle>
            {
                new() { RegistrationNumber = "ABC123", VehicleType = VehicleType.Car,  Color = "Red",    Model = "Corolla", NumberOfWheels = 4, BrandType = BrandType.Toyota,       ArrivedTime = now.AddDays(-2).AddHours(-3) },
                new() { RegistrationNumber = "DEF456", VehicleType = VehicleType.Car,  Color = "Blue",   Model = "Civic",   NumberOfWheels = 4, BrandType = BrandType.Honda,        ArrivedTime = now.AddDays(-1).AddHours(-5) },
                new() { RegistrationNumber = "GHI789", VehicleType = VehicleType.Car,  Color = "Black",  Model = "Focus",   NumberOfWheels = 4, BrandType = BrandType.Ford,         ArrivedTime = now.AddHours(-26) },
                new() { RegistrationNumber = "JKL012", VehicleType = VehicleType.Car,  Color = "White",  Model = "Golf",    NumberOfWheels = 4, BrandType = BrandType.Volkswagen,   ArrivedTime = now.AddHours(-8) },
                new() { RegistrationNumber = "MNO345", VehicleType = VehicleType.Car,  Color = "Silver", Model = "X5",      NumberOfWheels = 4, BrandType = BrandType.BMW,          ArrivedTime = now.AddHours(-4).AddMinutes(-30) },
                new() { RegistrationNumber = "PQR678", VehicleType = VehicleType.Bus,  Color = "Yellow", Model = "Sprinter",NumberOfWheels = 6, BrandType = BrandType.MercedesBenz, ArrivedTime = now.AddHours(-3) },
                new() { RegistrationNumber = "STU901", VehicleType = VehicleType.Bus,  Color = "Green",  Model = "Transit", NumberOfWheels = 6, BrandType = BrandType.Ford,         ArrivedTime = now.AddHours(-2).AddMinutes(-15) },
                new() { RegistrationNumber = "VWX234", VehicleType = VehicleType.Boat, Color = "White",  Model = "Nautica", NumberOfWheels = 2, BrandType = BrandType.Nissan,       ArrivedTime = now.AddMinutes(-90) },
                new() { RegistrationNumber = "YZA567", VehicleType = VehicleType.Car,  Color = "Gray",   Model = "Elantra", NumberOfWheels = 4, BrandType = BrandType.Hyundai,      ArrivedTime = now.AddMinutes(-45) },
                new() { RegistrationNumber = "BCD890", VehicleType = VehicleType.Car,  Color = "Orange", Model = "Sportage",NumberOfWheels = 4, BrandType = BrandType.Kia,          ArrivedTime = now.AddMinutes(-10) },
            };

            context.ParkedVehicles.AddRange(vehicles);
            context.SaveChanges();
        }
    }
}