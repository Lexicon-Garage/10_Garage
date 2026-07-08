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
    }
}
