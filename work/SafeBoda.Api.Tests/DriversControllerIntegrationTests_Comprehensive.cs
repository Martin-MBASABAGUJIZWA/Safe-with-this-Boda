using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SafeBoda.Core;
using SafeBoda.Infrastructure;
using Xunit;

namespace SafeBoda.Api.Tests
{
    public class DriversControllerIntegrationTests_Comprehensive : IAsyncLifetime
    {
        private WebApplicationFactory<Program>? _factory;
        private HttpClient? _client;

        public async Task InitializeAsync()
        {
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SafeBodaDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    // Add in-memory database with unique name per test
                    var dbName = $"SafeBoda_Drivers_{Guid.NewGuid()}";
                    services.AddDbContext<SafeBodaDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            _factory?.Dispose();
            await Task.CompletedTask;
        }

        // GetAllDrivers Tests
        [Fact]
        public async Task GetAllDrivers_ReturnsOk_WithEmptyList()
        {
            // Act
            var response = await _client!.GetAsync("/api/drivers");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var drivers = JsonSerializer.Deserialize<IEnumerable<Driver>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(drivers);
            Assert.Empty(drivers);
        }

        [Fact]
        public async Task PostCreateDriver_ReturnsCreatedAtAction_WithValidDriver()
        {
            // Arrange
            var newDriver = new { name = "John Doe", phoneNumber = "0701234567", motoPlateNumber = "UBE123" };
            var json = JsonSerializer.Serialize(newDriver);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PostAsync("/api/drivers", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdDriver = JsonSerializer.Deserialize<Driver>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(createdDriver);
            Assert.Equal("John Doe", createdDriver.Name);
            Assert.Equal("0701234567", createdDriver.PhoneNumber);
            Assert.Equal("UBE123", createdDriver.MotoPlateNumber);
        }

        [Fact]
        public async Task PostCreateDriver_ReturnsBadRequest_WhenDriverIsNull()
        {
            // Arrange
            var content = new StringContent("null", System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PostAsync("/api/drivers", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetDriverById_ReturnsOk_WithValidId()
        {
            // Arrange
            var newDriver = new { name = "John Doe", phoneNumber = "0701234567", motoPlateNumber = "UBE123" };
            var json = JsonSerializer.Serialize(newDriver);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var createResponse = await _client!.PostAsync("/api/drivers", createContent);
            var createdDriver = JsonSerializer.Deserialize<Driver>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Act
            var response = await _client!.GetAsync($"/api/drivers/{createdDriver!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var driver = JsonSerializer.Deserialize<Driver>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(driver);
            Assert.Equal("John Doe", driver.Name);
        }

        [Fact]
        public async Task GetDriverById_ReturnsNotFound_WithInvalidId()
        {
            // Act
            var response = await _client!.GetAsync($"/api/drivers/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PutUpdateDriver_ReturnsNoContent_WithValidDriver()
        {
            // Arrange
            var newDriver = new { name = "John Doe", phoneNumber = "0701234567", motoPlateNumber = "UBE123" };
            var json = JsonSerializer.Serialize(newDriver);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var createResponse = await _client!.PostAsync("/api/drivers", createContent);
            var createdDriver = JsonSerializer.Deserialize<Driver>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var updatedDriver = new { id = createdDriver!.Id, name = "John Updated", phoneNumber = "0701234567", motoPlateNumber = "UBE123" };
            var updateJson = JsonSerializer.Serialize(updatedDriver);
            var updateContent = new StringContent(updateJson, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PutAsync($"/api/drivers/{createdDriver.Id}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the update
            var verifyResponse = await _client!.GetAsync($"/api/drivers/{createdDriver.Id}");
            var verifyContent = await verifyResponse.Content.ReadAsStringAsync();
            var verifiedDriver = JsonSerializer.Deserialize<Driver>(verifyContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.Equal("John Updated", verifiedDriver!.Name);
        }

        [Fact]
        public async Task PutUpdateDriver_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var newDriver = new { name = "John Doe", phoneNumber = "0701234567", motoPlateNumber = "UBE123" };
            var json = JsonSerializer.Serialize(newDriver);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var createResponse = await _client!.PostAsync("/api/drivers", createContent);
            var createdDriver = JsonSerializer.Deserialize<Driver>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var differentId = Guid.NewGuid();
            var updatedDriver = new { id = differentId, name = "John Updated", phoneNumber = "0701234567", motoPlateNumber = "UBE123" };
            var updateJson = JsonSerializer.Serialize(updatedDriver);
            var updateContent = new StringContent(updateJson, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PutAsync($"/api/drivers/{createdDriver!.Id}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteDriver_ReturnsNoContent()
        {
            // Arrange
            var newDriver = new { name = "John Doe", phoneNumber = "0701234567", motoPlateNumber = "UBE123" };
            var json = JsonSerializer.Serialize(newDriver);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var createResponse = await _client!.PostAsync("/api/drivers", createContent);
            var createdDriver = JsonSerializer.Deserialize<Driver>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Act
            var response = await _client!.DeleteAsync($"/api/drivers/{createdDriver!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify deletion
            var verifyResponse = await _client!.GetAsync($"/api/drivers/{createdDriver.Id}");
            Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
        }

        [Fact]
        public async Task PostCreateMultipleDrivers_ReturnsOkWithAllDrivers()
        {
            // Arrange & Act
            var driver1Data = new { name = "John Doe", phoneNumber = "0701234567", motoPlateNumber = "UBE123" };
            var driver2Data = new { name = "Jane Smith", phoneNumber = "0702345678", motoPlateNumber = "UBE456" };

            await _client!.PostAsync("/api/drivers",
                new StringContent(JsonSerializer.Serialize(driver1Data),
                    System.Text.Encoding.UTF8, "application/json"));

            await _client!.PostAsync("/api/drivers",
                new StringContent(JsonSerializer.Serialize(driver2Data),
                    System.Text.Encoding.UTF8, "application/json"));

            // Act
            var response = await _client!.GetAsync("/api/drivers");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var drivers = JsonSerializer.Deserialize<IEnumerable<Driver>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(drivers);
            Assert.Equal(2, drivers.Count());
        }

        [Fact]
        public async Task PutUpdateDriver_ReturnsBadRequest_WhenDriverIsNull()
        {
            // Arrange
            var newDriver = new { name = "John Doe", phoneNumber = "0701234567", motoPlateNumber = "UBE123" };
            var json = JsonSerializer.Serialize(newDriver);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var createResponse = await _client!.PostAsync("/api/drivers", createContent);
            var createdDriver = JsonSerializer.Deserialize<Driver>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var updateContent = new StringContent("null", System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PutAsync($"/api/drivers/{createdDriver!.Id}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteDriver_DoesNotAffectOtherDrivers()
        {
            // Arrange
            var driver1Data = new { name = "John Doe", phoneNumber = "0701234567", motoPlateNumber = "UBE123" };
            var driver2Data = new { name = "Jane Smith", phoneNumber = "0702345678", motoPlateNumber = "UBE456" };

            var create1Response = await _client!.PostAsync("/api/drivers",
                new StringContent(JsonSerializer.Serialize(driver1Data),
                    System.Text.Encoding.UTF8, "application/json"));
            var driver1 = JsonSerializer.Deserialize<Driver>(
                await create1Response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var create2Response = await _client!.PostAsync("/api/drivers",
                new StringContent(JsonSerializer.Serialize(driver2Data),
                    System.Text.Encoding.UTF8, "application/json"));
            var driver2 = JsonSerializer.Deserialize<Driver>(
                await create2Response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Act
            await _client!.DeleteAsync($"/api/drivers/{driver1!.Id}");

            // Assert
            var response = await _client!.GetAsync($"/api/drivers/{driver2!.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CachingBehavior_ReturnsConsistentData()
        {
            // Arrange
            var newDriver = new { name = "John Doe", phoneNumber = "0701234567", motoPlateNumber = "UBE123" };
            var json = JsonSerializer.Serialize(newDriver);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await _client!.PostAsync("/api/drivers", createContent);

            // Act: Make multiple requests
            var response1 = await _client!.GetAsync("/api/drivers");
            var response2 = await _client!.GetAsync("/api/drivers");

            // Assert: Both should return same data
            var content1 = await response1.Content.ReadAsStringAsync();
            var content2 = await response2.Content.ReadAsStringAsync();

            var drivers1 = JsonSerializer.Deserialize<IEnumerable<Driver>>(content1,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var drivers2 = JsonSerializer.Deserialize<IEnumerable<Driver>>(content2,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal(drivers1!.Count(), drivers2!.Count());
            Assert.Single(drivers1);
        }
    }
}
