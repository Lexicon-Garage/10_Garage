using Garage.Web.Data;
using Garage.Web.Helper;
using Garage.Web.Models;
using Garage.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Garage.Test
{
    public class EditParkedVehicleTests : IClassFixture<TestFixture>
    {
        private readonly AppDbConext _context;
        public EditParkedVehicleTests(TestFixture testFixture)
        {
            _context= testFixture.Context;
        }

        [Fact]
        public async Task EditParkedVehicle_Should_ReturnNotFoundWhenNullParameter()
        {
            // Arrange
            var controller = new ParkedVehiclesController(_context);

            // Act
            var result = await controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task EditParkedVehicle_Should_ReturnNotFoundWhenVehicleNotExsist()
        {
            // Arrange
            var controller = new ParkedVehiclesController(_context);

            // Act
            var result = await controller.Edit(10);
            

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task EditParkedVehicle_Should_ReturnParkedVehicleEditViewModel()
        {
            // Arrange
            var controller = new ParkedVehiclesController(_context);

            // Act
            var result = await controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var parkedVehicleEditViewModel = Assert.IsType<ParkedVehicleEditViewModel>(viewResult.Model);

            Assert.Equal(1, parkedVehicleEditViewModel.Id);
        }

        [Fact]
        public async Task Edit_Should_ReturnError_WhenModelStateIsInvalid()
        {
            // Arrange
            var controller = new ParkedVehiclesController(_context);

            controller.ModelState.AddModelError("Color", "Color is required");

            var model = new ParkedVehicleEditViewModel
            {
                Id = 1,
                RegistrationNumber = "ABC123",
                BrandType = BrandType.Toyota,
                Model = "XC60",
                Color = "",
                NumberOfWheels = 4,
                ArrivedTime = DateTime.Now,
                VehicleType = VehicleType.Car
            };

            // Act
            var result = await controller.Edit(model.Id, model);

            // Assert
            Assert.False(controller.ModelState.IsValid);

            var viewResult = Assert.IsType<ViewResult>(result);

            var returnedModel = Assert.IsType<ParkedVehicleEditViewModel>(viewResult.Model);

            Assert.Equal("", returnedModel.Color);
        }
    }
}
