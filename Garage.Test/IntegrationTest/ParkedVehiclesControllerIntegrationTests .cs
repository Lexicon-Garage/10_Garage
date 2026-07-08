using Garage.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Garage.Test.IntegrationTest
{
    public class ParkedVehiclesControllerIntegrationTests
      : IClassFixture<GarageWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ParkedVehiclesControllerIntegrationTests(
          GarageWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }


        [Fact]
        public async Task Edit_Should_ShowValidationError_WhenColorIsEmpty()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "Id", "1" },
                { "RegistrationNumber", "ABC123" },
                { "VehicleType", "Car" },
                { "Color", "" },
                { "NumberOfWheels", "4" },
                { "Model", "XC60" },
                { "BrandType", "Toyota" },
                { "ArrivedTime", DateTime.Now.ToString() }
            };

            var content = new FormUrlEncodedContent(formData);

            // Act
            var response = await _client.PostAsync(
                "/ParkedVehicles/Edit/1",
                content);

            var html = await response.Content.ReadAsStringAsync();


            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Contains(
                "The Color field is required.",
                html);
        }
        [Fact]
        public async Task Edit_Should_ShowValidationError_WhenRegistrationNumberIsEmpty()
        {
            // Arrange
            var formData = new Dictionary<string, string>
        {
            { "Id", "1" },
            { "RegistrationNumber", "" },
            { "VehicleType", "Car" },
            { "Color", "Blue" },
            { "NumberOfWheels", "4" },
            { "Model", "XC60" },
            { "BrandType", "Toyota" },
            { "ArrivedTime", DateTime.Now.ToString("O") }
        };

            var content = new FormUrlEncodedContent(formData);

            // Act
            var response = await _client.PostAsync("/ParkedVehicles/Edit/1", content);
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("The Registration Number field is required.", html);
        }

        [Fact]
        public async Task Edit_Should_ShowValidationError_WhenModelIsEmpty()
        {
            // Arrange
            var formData = new Dictionary<string, string>
        {
            { "Id", "1" },
            { "RegistrationNumber", "ABC123" },
            { "VehicleType", "Car" },
            { "Color", "Blue" },
            { "NumberOfWheels", "4" },
            { "Model", "" },
            { "BrandType", "Toyota" },
            { "ArrivedTime", DateTime.Now.ToString("O") }
        };

            var content = new FormUrlEncodedContent(formData);

            // Act
            var response = await _client.PostAsync("/ParkedVehicles/Edit/1", content);
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("The Model field is required.", html);
        }

        [Fact]
        public async Task Edit_Should_ShowValidationError_WhenBrandIsEmpty()
        {
            // Arrange
            var formData = new Dictionary<string, string>
        {
            { "Id", "1" },
            { "RegistrationNumber", "ABC123" },
            { "VehicleType", "Car" },
            { "Color", "Blue" },
            { "NumberOfWheels", "4" },
            { "Model", "XC60" },
            { "BrandType", "" },
            { "ArrivedTime", DateTime.Now.ToString("O") }
        };

            var content = new FormUrlEncodedContent(formData);

            // Act
            var response = await _client.PostAsync("/ParkedVehicles/Edit/1", content);
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("The Brand field is required.", html);
        }

        [Fact]
        public async Task Edit_Should_ShowValidationError_WhenNumberOfWheelsIsInvalid()
        {
            // Arrange
            var formData = new Dictionary<string, string>
        {
            { "Id", "1" },
            { "RegistrationNumber", "ABC123" },
            { "VehicleType", "Car" },
            { "Color", "Blue" },
            { "NumberOfWheels", "0" }, // or "-1" depending on your validation
            { "Model", "XC60" },
            { "BrandType", "Toyota" },
            { "ArrivedTime", DateTime.Now.ToString("O") }
        };

            var content = new FormUrlEncodedContent(formData);

            // Act
            var response = await _client.PostAsync("/ParkedVehicles/Edit/1", content);
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Adjust this message to match your validation attribute
            Assert.Contains("The field Number of wheels must be between 1 and 16.", html);
        }
    }
}
