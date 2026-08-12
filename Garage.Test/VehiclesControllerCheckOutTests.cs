using Garage.Test.Fixtures;
using Garage.Test.Helpers;
using Garage.Web.Models;
using Garage.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Garage.Test;

public class VehiclesControllerCheckOutTests : IClassFixture<ControllerTestFixture>
{
    private readonly VehiclesTestFactory _factory;

    public VehiclesControllerCheckOutTests(ControllerTestFixture fixture)
    {
        _factory = new VehiclesTestFactory(fixture);
    }

    [Fact]
    public async Task CheckOut_Get_NullId_ReturnsNotFound()
    {
        await using var context = _factory.CreateContext();

        var controller = _factory.CreateController(
            context,
            "user-1");

        var result = await controller.CheckOut(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CheckOut_Get_OtherUsersVehicle_ReturnsNotFound()
    {
        await using var context =
            await _factory.CreateContextWithVehicles(
                _factory.CreateVehicle(
                    1,
                    "ABC123",
                    "user-2"));

        var controller = _factory.CreateController(
            context,
            "user-1");

        var result = await controller.CheckOut(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CheckOut_Get_OwnVehicle_ReturnsVehicle()
    {
        await using var context = _factory.CreateContext();
        context.BrandTypes.AddRange(
            _factory.CreateBrandType(
                1,
                "Volkswagen"),
            _factory.CreateBrandType(
                2,
                "BMW"));

        context.VehicleTypes.Add(
            _factory.CreateVehicleType(
                1,
                "Car"));

        context.Vehicles.Add(
            _factory.CreateVehicle(
                1,
                "ABC123",
                "user-1",
                brandTypeId: 1));

        await context.SaveChangesAsync();

        var controller = _factory.CreateController(
            context,
            "user-1");

        var result = await controller.CheckOut((int?)1);

        var viewResult = Assert.IsType<ViewResult>(result);

        var model = Assert.IsType<Vehicle>(viewResult.Model);

        Assert.Equal(
            "ABC123",
            model.RegistrationNumber);
    }

    [Fact]
    public async Task CheckOut_Post_InvalidId_ReturnsNotFound()
    {
        await using var context = _factory.CreateContext();

        var controller = _factory.CreateController(
            context,
            "user-1");

        var result = await controller.CheckOut(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CheckOut_Post_OtherUsersVehicle_ReturnsNotFound()
    {
        await using var context =
            await _factory.CreateContextWithVehicles(
                _factory.CreateVehicle(
                    1,
                    "ABC123",
                    "user-2"));

        var controller = _factory.CreateController(
            context,
            "user-1");

        var result = await controller.CheckOut(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CheckOut_Post_ValidVehicle_RemovesVehicleAndCreatesReceipt()
    {
        await using var context = _factory.CreateContext();
        context.BrandTypes.AddRange(
            _factory.CreateBrandType(
                1,
                "Volkswagen"),
            _factory.CreateBrandType(
                2,
                "BMW"));    


        context.VehicleTypes.Add(
            _factory.CreateVehicleType(
                1,
                "Car"));

        context.Vehicles.Add(
            _factory.CreateVehicle(
                1,
                "ABC123",
                "user-1"));

        await context.SaveChangesAsync();

        var controller = _factory.CreateController(
            context,
            "user-1");

        var result = await controller.CheckOut(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Receipts", redirect.ControllerName);

        Assert.Empty(context.Vehicles);

        Assert.Equal(
            "The vehicle has been checked out successfully.",
            controller.TempData["ValidationMessage"]);

        var receiptJson = controller.TempData["Receipt"]?.ToString();

        Assert.NotNull(receiptJson);

        var receipt =
            JsonSerializer.Deserialize<ReceiptViewModel>(
                receiptJson!);

        Assert.NotNull(receipt);

        Assert.Equal(
            "ABC123",
            receipt!.RegistrationNumber);

        Assert.Equal(
            "Car",
            receipt.VehicleType);
    }
}