using Garage.Test.Fixtures;
using Garage.Test.Helpers;
using Garage.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Garage.Test;

public class VehiclesControllerEditTests: IClassFixture<ControllerTestFixture>
{
    private readonly VehiclesTestFactory _factory;

    public VehiclesControllerEditTests(ControllerTestFixture fixture)
    {
        _factory =new VehiclesTestFactory(fixture);
    }

    [Fact]
    public async Task Edit_Get_NullId_ReturnsNotFound()
    {
        await using var context =
            _factory.CreateContext();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Edit(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_OtherUsersVehicle_ReturnsNotFound()
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
            await controller.Edit(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_OwnVehicle_ReturnsEditViewModel()
    {
        await using var context =
            _factory.CreateContext();

        context.VehicleTypes.AddRange(
            _factory.CreateVehicleType(
                1,
                "Car"),

            _factory.CreateVehicleType(
                2,
                "Motorcycle"));

        context.Vehicles.Add(
            _factory.CreateVehicle(
                1,
                "ABC123",
                "user-1",
                vehicleTypeId: 1));

        await context.SaveChangesAsync();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Edit(1);

        var viewResult =
            Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsType<VehicleEditViewModel>(
                viewResult.Model);

        Assert.Equal(
            1,
            model.Id);

        Assert.Equal(
            "ABC123",
            model.RegistrationNumber);

        Assert.Equal(
            1,
            model.VehicleTypeId);

        Assert.Equal(
            2,
            model.VehicleTypes.ToList().Count);

        Assert.Contains(
            model.VehicleTypes,
            x => x.Value == "1" &&
                 x.Selected);
    }

    [Fact]
    public async Task Edit_Post_InvalidModelState_ReturnsViewWithVehicleTypes()
    {
        await using var context =
            _factory.CreateContext();

        context.VehicleTypes.AddRange(
            _factory.CreateVehicleType(
                1,
                "Car"),

            _factory.CreateVehicleType(
                2,
                "Motorcycle"));

        await context.SaveChangesAsync();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        controller.ModelState.AddModelError(
            "Color",
            "Color is required.");

        var model =
            new VehicleEditViewModel
            {
                Id = 1,
                VehicleTypeId = 2,
                RegistrationNumber = "ABC123",
                Color = "",
                Model = "Golf"
            };

        var result =
            await controller.Edit(
                1,
                model);

        var viewResult =
            Assert.IsType<ViewResult>(result);

        var returnedModel =
            Assert.IsType<VehicleEditViewModel>(
                viewResult.Model);

        Assert.Equal(
            2,
            returnedModel.VehicleTypes.ToList().Count);

        Assert.Contains(
            returnedModel.VehicleTypes,
            x => x.Value == "2" &&
                 x.Selected);
    }

    [Fact]
    public async Task Edit_Post_NullId_ReturnsNotFound()
    {
        await using var context =
            _factory.CreateContext();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var model =
            new VehicleEditViewModel
            {
                RegistrationNumber = "ABC123",
                Color = "Red",
                Model = "Golf"
            };

        var result =
            await controller.Edit(
                null,
                model);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_InvalidId_ReturnsNotFound()
    {
        await using var context =
            _factory.CreateContext();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var model =
            new VehicleEditViewModel
            {
                RegistrationNumber = "ABC123",
                VehicleTypeId = 1,
                Color = "Red",
                Model = "Golf"
            };

        var result =
            await controller.Edit(
                999,
                model);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_DuplicateRegistration_ReturnsViewWithError()
    {
        await using var context =
            _factory.CreateContext();

        context.VehicleTypes.Add(
            _factory.CreateVehicleType());

        context.Vehicles.AddRange(
            _factory.CreateVehicle(
                1,
                "ABC123",
                "user-1"),

            _factory.CreateVehicle(
                2,
                "XYZ999",
                "user-1"));

        await context.SaveChangesAsync();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var model =
            new VehicleEditViewModel
            {
                RegistrationNumber = " abc123 ",
                VehicleTypeId = 1,
                Color = "Blue",
                Model = "Golf",
                BrandTypeId = 1,
                NumberOfWheels = 4
            };

        var result =
            await controller.Edit(
                2,
                model);

        var viewResult =
            Assert.IsType<ViewResult>(result);

        Assert.True(
            controller.ModelState.ContainsKey(
                "RegistrationNumber"));

        Assert.Contains(
            "ABC123",
            controller.ModelState[
                    "RegistrationNumber"]!
                .Errors
                .Single()
                .ErrorMessage);

        Assert.NotNull(
            ((VehicleEditViewModel)
                viewResult.Model!)
                .VehicleTypes);
    }

    [Fact]
    public async Task Edit_Post_ValidVehicle_UpdatesVehicle()
    {
        await using var context =
            await _factory.CreateContextWithVehicles(
                _factory.CreateVehicle(
                    1,
                    "ABC123",
                    "user-1"));

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var model =
            new VehicleEditViewModel
            {
                RegistrationNumber = " xyz999 ",
                VehicleTypeId = 1,
                Color = " Blue ",
                Model = " Passat ",
                BrandTypeId = 1,
                NumberOfWheels = 4
            };

        var result =
            await controller.Edit(
                1,
                model);

        var redirect =
            Assert.IsType<RedirectToActionResult>(
                result);

        Assert.Equal(
            nameof(VehiclesController.Index),
            redirect.ActionName);

        var vehicle =
            await context.Vehicles.SingleAsync();

        Assert.Equal(
            "XYZ999",
            vehicle.RegistrationNumber);

        Assert.Equal(
            "Blue",
            vehicle.Color);

        Assert.Equal(
            "Passat",
            vehicle.Model);

        Assert.Equal(
            "The vehicle has been updated successfully.",
            controller.TempData["ValidationMessage"]);
    }
}