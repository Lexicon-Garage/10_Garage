using Garage.Web.Data;
using Garage.Web.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Garage.Test
{
    public static class TestDataSeeder
    {
        public static void Seed(AppDbConext context)
        {
            context.ParkedVehicles.AddRange(
                new ParkedVehicle
                {
                    Id = 1,
                    RegistrationNumber = "ABC123",
                    BrandType = BrandType.Toyota,
                    Model = "XC60",
                    Color = "Black",
                    NumberOfWheels = 4,
                    ArrivedTime = DateTime.Now,
                    VehicleType = VehicleType.Car
                },
                new ParkedVehicle
                {
                    Id = 2,
                    RegistrationNumber = "XYZ789",
                    BrandType = BrandType.MercedesBenz,
                    Model = "Model Y",
                    Color = "White",
                    NumberOfWheels = 4,
                    ArrivedTime = DateTime.Now,
                    VehicleType = VehicleType.Car
                },
                  new ParkedVehicle
                  {
                      Id = 3,
                      RegistrationNumber = "XYZ889",
                      BrandType = BrandType.MercedesBenz,
                      Model = "Model Y",
                      Color = "Red",
                      NumberOfWheels = 4,
                      ArrivedTime = DateTime.Now,
                      VehicleType = VehicleType.Bus
                  });

            context.SaveChanges();
        }
    }
}

