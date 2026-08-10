using Garage.Test.Fixtures;
using Garage.Web.Controllers;
using Garage.Web.Data;
using Garage.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Garage.Test.Helpers;

public class VehiclesTestFactory
{
    private readonly ControllerTestFixture _fixture;

    public VehiclesTestFactory(ControllerTestFixture fixture)
       
    {
        _fixture = fixture;
    }

    public AppDbConext CreateContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbConext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new AppDbConext(options);
    }

    public VehiclesController CreateController(AppDbConext context,string userId = "user-1", bool isAdmin = false)
    {
        var controller =
            new VehiclesController(
                context,
                _fixture.UserManagerMock.Object);

        _fixture.SetupUser(
            controller,
            userId,
            isAdmin);

        return controller;
    }

    public VehiclesController CreateControllerWithoutUser(AppDbConext context)
        
    {
        var controller =
            new VehiclesController(
                context,
                _fixture.UserManagerMock.Object);

        _fixture.SetupNoUser(controller);

        return controller;
    }

    public VehicleType CreateVehicleType(int id = 1,string name = "Car")
    {
        return new VehicleType
        {
            Id = id,
            Name = name
        };
    }

    public Vehicle CreateVehicle(
        int id,
        string registrationNumber,
        string ownerId,
        int vehicleTypeId = 1,
        string color = "Red",
        string model = "Golf",
        string brand = "Volkswagen",
        int wheels = 4
       )
    {
        return new Vehicle
        {
            Id = id,
            RegistrationNumber = registrationNumber,
            OwnerId = ownerId,
            VehicleTypeId = vehicleTypeId,
            Color = color,
            Model = model,
            Brand = brand,
            NumberOfWheels = wheels
        };
    }

    public async Task<AppDbConext> CreateContextWithVehicles( params Vehicle[] vehicles)
    {
        var context = CreateContext();

        context.VehicleTypes.Add(
            CreateVehicleType());

        context.Vehicles.AddRange(vehicles);

        await context.SaveChangesAsync();

        return context;
    }

    public async Task<AppDbConext> CreateContextWithVehicleTypes(params VehicleType[] vehicleTypes)
    {
        var context = CreateContext();

        context.VehicleTypes.AddRange(vehicleTypes);

        await context.SaveChangesAsync();

        return context;
    }
}