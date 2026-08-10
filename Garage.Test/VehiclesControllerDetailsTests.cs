using Garage.Test.Fixtures;
using Garage.Test.Helpers;
using Garage.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Garage.Test;

public class VehiclesControllerDetailsTests: IClassFixture<ControllerTestFixture>
{
    private readonly VehiclesTestFactory _factory;

    public VehiclesControllerDetailsTests(ControllerTestFixture fixture)
    {
        _factory =new VehiclesTestFactory(fixture);
    }

    [Fact]
    public async Task Details_NullId_ReturnsNotFound()
    {
        await using var context =
            _factory.CreateContext();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_InvalidId_ReturnsNotFound()
    {
        await using var context =
            _factory.CreateContext();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_OtherUsersVehicle_ReturnsNotFound()
    {
        await using var context =
            await _factory.CreateContextWithVehicles(
                _factory.CreateVehicle(
                    1,
                    "ABC123",
                    "user-2"));

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Details(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_OwnVehicle_ReturnsCorrectViewModel()
    {
        await using var context =
            _factory.CreateContext();

        context.VehicleTypes.Add(
            _factory.CreateVehicleType(
                1,
                "Car"));

        context.Vehicles.Add(
            _factory.CreateVehicle(
                1,
                "ABC123",
                "user-1",
                color: "Red",
                model: "Golf",
                brand: "Volkswagen",
                wheels: 4));

        await context.SaveChangesAsync();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Details(1);

        var viewResult =
            Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsType<VehicleDetailsViewModel>(
                viewResult.Model);

        Assert.Equal(
            1,
            model.Id);

        Assert.Equal(
            "ABC123",
            model.RegistrationNumber);

        Assert.Equal(
            "Car",
            model.VehicleType);

        Assert.Equal(
            "Red",
            model.Color);

        Assert.Equal(
            "Golf",
            model.Model);

        Assert.Equal(
            "Volkswagen",
            model.Brand);

        Assert.Equal(
            4,
            model.NumberOfWheels);
    }
}