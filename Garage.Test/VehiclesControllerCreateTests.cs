using Garage.Test.Fixtures;
using Garage.Test.Helpers;
using Garage.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Garage.Test;

public class VehiclesControllerCreateTests: IClassFixture<ControllerTestFixture>
{
    private readonly VehiclesTestFactory _factory;
    private readonly FailingDbContextFactory _failingFactory;

    public VehiclesControllerCreateTests(ControllerTestFixture fixture)
    {
        _factory = new VehiclesTestFactory(fixture);
        _failingFactory = new FailingDbContextFactory();
    }

    [Fact]
    public async Task Create_Get_ReturnsVehicleTypes()
    {
        await using var context = _factory.CreateContext();

        context.VehicleTypes.AddRange(
            _factory.CreateVehicleType(
                1,
                "Car"),
            _factory.CreateVehicleType(
                2,
                "Motorcycle"));

        await context.SaveChangesAsync();

        var controller = _factory.CreateController(
            context,
            "user-1");

        var result = await controller.Create();

        var viewResult = Assert.IsType<ViewResult>(result);

        var model = Assert.IsType<CreateVehicleViewModel>(
            viewResult.Model);

        Assert.Equal(
            2,
            model.VehicleTypes.ToList().Count);

        Assert.Contains(
            model.VehicleTypes,
            x => x.Text == "Car");

        Assert.Contains(
            model.VehicleTypes,
            x => x.Text == "Motorcycle");
    }

    [Fact]
    public async Task Create_Post_ValidVehicle_CreatesVehicleAndRedirects()
    {
        await using var context = _factory.CreateContext();

        context.VehicleTypes.Add(
            _factory.CreateVehicleType(
                1,
                "Car"));

        await context.SaveChangesAsync();

        var controller = _factory.CreateController(
            context,
            "user-1");

        var model = new CreateVehicleViewModel
        {
            VehicleTypeId = 1,
            RegistrationNumber = " abc123 ",
            Color = " Red ",
            BrandTypeId = 1,
            Model = " Golf ",
            WheelsCount = 4
        };

        var result = await controller.Create(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(
            nameof(VehiclesController.Index),
            redirect.ActionName);

        var vehicle = await context.Vehicles.SingleAsync();

        Assert.Equal(
            "ABC123",
            vehicle.RegistrationNumber);

        Assert.Equal(
            "Red",
            vehicle.Color);

        Assert.Equal(
            "Golf",
            vehicle.Model);

        Assert.Equal(
            "user-1",
            vehicle.OwnerId);

        Assert.Equal(
            "The vehicle has been successfully added.",
            controller.TempData["ValidationMessage"]);
    }

    [Fact]
    public async Task Create_Post_DuplicateRegistration_ReturnsViewWithError()
    {
        await using var context = _factory.CreateContext();

        context.VehicleTypes.Add(
            _factory.CreateVehicleType());

        context.BrandTypes.Add(
            _factory.CreateBrandType());

        context.Vehicles.Add(
            _factory.CreateVehicle(
                1,
                "ABC123",
                "user-2"));

        await context.SaveChangesAsync();

        var controller = _factory.CreateController(
            context,
            "user-1");

        var model = new CreateVehicleViewModel
        {
            VehicleTypeId = 1,
            RegistrationNumber = " abc123 ",
            Color = "Red",
            BrandTypeId = 1,
            Model = "XC60",
            WheelsCount = 4
        };

        var result = await controller.Create(model);

        Assert.IsType<ViewResult>(result);

        Assert.Equal(
            "A vehicle with this registration number is already exists.",
            controller.ModelState["RegistrationNumber"]!
                .Errors
                .Single()
                .ErrorMessage);
    }

    [Fact]
    public async Task Create_Post_InvalidModelState_ReturnsView()
    {
        await using var context = _factory.CreateContext();

        var controller = _factory.CreateController(
            context,
            "user-1");

        controller.ModelState.AddModelError(
            "RegistrationNumber",
            "Registration number is required.");

        var model = new CreateVehicleViewModel
        {
            RegistrationNumber = "ABC123"
        };

        var result = await controller.Create(model);

        Assert.IsType<ViewResult>(result);

        Assert.Empty(context.Vehicles);
    }

    [Fact]
    public async Task Create_Post_NoUserId_ReturnsUnauthorized()
    {
        await using var context = _factory.CreateContext();

        var controller = _factory.CreateControllerWithoutUser(
            context);

        var model = new CreateVehicleViewModel
        {
            VehicleTypeId = 1,
            RegistrationNumber = "ABC123",
            Color = "Red",
            BrandTypeId = 1,
            Model = "XC60",
            WheelsCount = 4
        };

        var result = await controller.Create(model);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Create_Post_DatabaseError_ReturnsViewWithErrorMessage()
    {
        // Arrange - seed the database using a normal context
        var failingFactory = new FailingDbContextFactory();

        await using (var seedContext =
            failingFactory.CreateNormalContext())
        {
            seedContext.VehicleTypes.Add(
                _factory.CreateVehicleType(
                    1,
                    "Car"));

            await seedContext.SaveChangesAsync();
        }

        // Create a new context that fails on SaveChangesAsync
        await using var context =
            failingFactory.CreateFailingContext();

        var controller = _factory.CreateController(
            context,
            "user-1");

        var model = new CreateVehicleViewModel
        {
            VehicleTypeId = 1,
            RegistrationNumber = "ABC123",
            Color = "Red",
            BrandTypeId = 1,
            Model = "XC60", 
            WheelsCount = 4
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        Assert.IsType<ViewResult>(result);

        Assert.Equal(
            "Could not save the vehicle data. Please try again.",
            controller.TempData["ValidationMessage"]);
    }
}