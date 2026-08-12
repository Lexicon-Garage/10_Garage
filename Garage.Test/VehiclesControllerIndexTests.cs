using Garage.Test.Fixtures;
using Garage.Test.Helpers;
using Garage.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Garage.Test;

public class VehiclesControllerIndexTests: IClassFixture<ControllerTestFixture>
{
    private readonly VehiclesTestFactory _factory;

    public VehiclesControllerIndexTests(ControllerTestFixture fixture)
    {
        _factory = new VehiclesTestFactory(fixture);
    }

    [Fact]
    public async Task Index_NonAdmin_ReturnsOnlyUsersOwnVehicles()
    {
        await using var context =
            _factory.CreateContext();

        context.VehicleTypes.Add(
            _factory.CreateVehicleType(
                1,
                "Car"));

        context.Vehicles.AddRange(
            _factory.CreateVehicle(
                1,
                "ABC123",
                "user-1"),

            _factory.CreateVehicle(
                2,
                "XYZ999",
                "user-2"));

        await context.SaveChangesAsync();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Index(
                null,
                null,
                null);

        var viewResult =
            Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsAssignableFrom<
                IEnumerable<VehicleOverviewViewModel>>(
                    viewResult.Model);

        var vehicles =
            model.ToList();

        Assert.Single(vehicles);

        Assert.Equal(
            "ABC123",
            vehicles[0].RegistrationNumber);
    }

    [Fact]
    public async Task Index_Admin_ReturnsAllVehicles()
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
                "user-2"));

        await context.SaveChangesAsync();

        var controller =
            _factory.CreateController(
                context,
                "admin-1",
                isAdmin: true);

        var result =
            await controller.Index(
                null,
                null,
                null);

        var viewResult =
            Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsAssignableFrom<
                IEnumerable<VehicleOverviewViewModel>>(
                    viewResult.Model);

        Assert.Equal(
            2,
            model.Count());
    }

    [Fact]
    public async Task Index_SearchByRegistrationNumber_ReturnsMatchingVehicle()
    {
        await using var context =
            await _factory.CreateContextWithVehicles(
                _factory.CreateVehicle(
                    1,
                    "ABC123",
                    "user-1"),

                _factory.CreateVehicle(
                    2,
                    "XYZ999",
                    "user-1"));

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Index(
                "ABC",
                null,
                null);

        var viewResult =
            Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsAssignableFrom<
                IEnumerable<VehicleOverviewViewModel>>(
                    viewResult.Model);

        Assert.Single(model);

        Assert.Equal(
            "ABC123",
            model.First().RegistrationNumber);
    }

    [Fact]
    public async Task Index_SearchByColor_ReturnsMatchingVehicle()
    {
        await using var context =
            _factory.CreateContext();

        context.VehicleTypes.Add(
            _factory.CreateVehicleType());

        context.Vehicles.AddRange(
            _factory.CreateVehicle(
                1,
                "ABC123",
                "user-1",
                color: "Red"),

            _factory.CreateVehicle(
                2,
                "XYZ999",
                "user-1",
                color: "Blue"));

        await context.SaveChangesAsync();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Index(
                "Red",
                null,
                null);

        var viewResult =
            Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsAssignableFrom<
                IEnumerable<VehicleOverviewViewModel>>(
                    viewResult.Model);

        Assert.Single(model);

        Assert.Equal(
            "ABC123",
            model.First().RegistrationNumber);
    }

    [Fact]
    public async Task Index_SearchByVehicleType_ReturnsMatchingVehicle()
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

        context.Vehicles.AddRange(
            _factory.CreateVehicle(
                1,
                "ABC123",
                "user-1",
                vehicleTypeId: 1),

            _factory.CreateVehicle(
                2,
                "XYZ999",
                "user-1",
                vehicleTypeId: 2));

        await context.SaveChangesAsync();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Index(
                "Motorcycle",
                null,
                null);

        var viewResult =
            Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsAssignableFrom<
                IEnumerable<VehicleOverviewViewModel>>(
                    viewResult.Model);

        Assert.Single(model);

        Assert.Equal(
            "XYZ999",
            model.First().RegistrationNumber);
    }

    [Fact]
    public async Task Index_SortByRegistrationAscending_SortsCorrectly()
    {
        await using var context =
            await _factory.CreateContextWithVehicles(
                _factory.CreateVehicle(
                    1,
                    "ZZZ",
                    "user-1"),

                _factory.CreateVehicle(
                    2,
                    "AAA",
                    "user-1"),

                _factory.CreateVehicle(
                    3,
                    "MMM",
                    "user-1"));

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Index(
                null,
                "reg",
                "asc");

        var viewResult =
            Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsAssignableFrom<
                IEnumerable<VehicleOverviewViewModel>>(
                    viewResult.Model);

        Assert.Equal(
            new[]
            {
                "AAA",
                "MMM",
                "ZZZ"
            },
            model.Select(
                x => x.RegistrationNumber));
    }

    [Fact]
    public async Task Index_SortByRegistrationDescending_SortsCorrectly()
    {
        await using var context =
            await _factory.CreateContextWithVehicles(
                _factory.CreateVehicle(
                    1,
                    "ZZZ",
                    "user-1"),

                _factory.CreateVehicle(
                    2,
                    "AAA",
                    "user-1"),

                _factory.CreateVehicle(
                    3,
                    "MMM",
                    "user-1"));

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Index(
                null,
                "reg",
                "desc");

        var viewResult =
            Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsAssignableFrom<
                IEnumerable<VehicleOverviewViewModel>>(
                    viewResult.Model);

        Assert.Equal(
            new[]
            {
                "ZZZ",
                "MMM",
                "AAA"
            },
            model.Select(
                x => x.RegistrationNumber));
    }

    [Fact]
    public async Task Index_NoSortColumn_SortsById()
    {
        await using var context =
            _factory.CreateContext();

        context.VehicleTypes.Add(
            _factory.CreateVehicleType());

        context.Vehicles.AddRange(
            _factory.CreateVehicle(
                3,
                "CCC",
                "user-1"),

            _factory.CreateVehicle(
                1,
                "AAA",
                "user-1"),

            _factory.CreateVehicle(
                2,
                "BBB",
                "user-1"));

        await context.SaveChangesAsync();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Index(
                null,
                null,
                null);

        var viewResult =
            Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsAssignableFrom<
                IEnumerable<VehicleOverviewViewModel>>(
                    viewResult.Model);

        Assert.Equal(
            new[] { 1, 2, 3 },
            model.Select(x => x.VehicleId));
    }

    [Fact]
    public async Task Index_SetsViewDataCorrectly()
    {
        await using var context =
            _factory.CreateContext();

        var controller =
            _factory.CreateController(
                context,
                "user-1");

        var result =
            await controller.Index(
                "abc",
                "reg",
                "desc");

        var viewResult =
            Assert.IsType<ViewResult>(result);

        Assert.Equal(
            "abc",
            viewResult.ViewData["SearchString"]);

        Assert.Equal(
            "reg",
            viewResult.ViewData["SortColumn"]);

        Assert.Equal(
            "desc",
            viewResult.ViewData["SortDir"]);
    }
}