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
    public class RidersControllerIntegrationTests_Comprehensive : IAsyncLifetime
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
                    var dbName = $"SafeBoda_Riders_{Guid.NewGuid()}";
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

        // GetAllRiders Tests
        [Fact]
        public async Task GetAllRiders_ReturnsOk_WithEmptyList()
        {
            // Act
            var response = await _client!.GetAsync("/api/riders");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var riders = JsonSerializer.Deserialize<IEnumerable<Rider>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(riders);
            Assert.Empty(riders);
        }

        [Fact]
        public async Task PostCreateRider_ReturnsCreatedAtAction_WithValidRider()
        {
            // Arrange
            var newRider = new { name = "John Doe", phoneNumber = "0701234567" };
            var json = JsonSerializer.Serialize(newRider);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PostAsync("/api/riders", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdRider = JsonSerializer.Deserialize<Rider>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(createdRider);
            Assert.Equal("John Doe", createdRider.Name);
            Assert.Equal("0701234567", createdRider.PhoneNumber);
        }

        [Fact]
        public async Task PostCreateRider_ReturnsBadRequest_WhenRiderIsNull()
        {
            // Arrange
            var content = new StringContent("null", System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PostAsync("/api/riders", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetRiderById_ReturnsOk_WithValidId()
        {
            // Arrange
            var newRider = new { name = "John Doe", phoneNumber = "0701234567" };
            var json = JsonSerializer.Serialize(newRider);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var createResponse = await _client!.PostAsync("/api/riders", createContent);
            var createdRider = JsonSerializer.Deserialize<Rider>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Act
            var response = await _client!.GetAsync($"/api/riders/{createdRider!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var rider = JsonSerializer.Deserialize<Rider>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(rider);
            Assert.Equal("John Doe", rider.Name);
        }

        [Fact]
        public async Task GetRiderById_ReturnsNotFound_WithInvalidId()
        {
            // Act
            var response = await _client!.GetAsync($"/api/riders/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PutUpdateRider_ReturnsNoContent_WithValidRider()
        {
            // Arrange
            var newRider = new { name = "John Doe", phoneNumber = "0701234567" };
            var json = JsonSerializer.Serialize(newRider);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var createResponse = await _client!.PostAsync("/api/riders", createContent);
            var createdRider = JsonSerializer.Deserialize<Rider>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var updatedRider = new { id = createdRider!.Id, name = "John Updated", phoneNumber = "0701234567" };
            var updateJson = JsonSerializer.Serialize(updatedRider);
            var updateContent = new StringContent(updateJson, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PutAsync($"/api/riders/{createdRider.Id}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the update
            var verifyResponse = await _client!.GetAsync($"/api/riders/{createdRider.Id}");
            var verifyContent = await verifyResponse.Content.ReadAsStringAsync();
            var verifiedRider = JsonSerializer.Deserialize<Rider>(verifyContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.Equal("John Updated", verifiedRider!.Name);
        }

        [Fact]
        public async Task PutUpdateRider_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var newRider = new { name = "John Doe", phoneNumber = "0701234567" };
            var json = JsonSerializer.Serialize(newRider);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var createResponse = await _client!.PostAsync("/api/riders", createContent);
            var createdRider = JsonSerializer.Deserialize<Rider>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var differentId = Guid.NewGuid();
            var updatedRider = new { id = differentId, name = "John Updated", phoneNumber = "0701234567" };
            var updateJson = JsonSerializer.Serialize(updatedRider);
            var updateContent = new StringContent(updateJson, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PutAsync($"/api/riders/{createdRider!.Id}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteRider_ReturnsNoContent()
        {
            // Arrange
            var newRider = new { name = "John Doe", phoneNumber = "0701234567" };
            var json = JsonSerializer.Serialize(newRider);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var createResponse = await _client!.PostAsync("/api/riders", createContent);
            var createdRider = JsonSerializer.Deserialize<Rider>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Act
            var response = await _client!.DeleteAsync($"/api/riders/{createdRider!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify deletion
            var verifyResponse = await _client!.GetAsync($"/api/riders/{createdRider.Id}");
            Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
        }

        [Fact]
        public async Task PostCreateMultipleRiders_ReturnsOkWithAllRiders()
        {
            // Arrange & Act
            var rider1Data = new { name = "John Doe", phoneNumber = "0701234567" };
            var rider2Data = new { name = "Jane Smith", phoneNumber = "0702345678" };

            await _client!.PostAsync("/api/riders",
                new StringContent(JsonSerializer.Serialize(rider1Data),
                    System.Text.Encoding.UTF8, "application/json"));

            await _client!.PostAsync("/api/riders",
                new StringContent(JsonSerializer.Serialize(rider2Data),
                    System.Text.Encoding.UTF8, "application/json"));

            // Act
            var response = await _client!.GetAsync("/api/riders");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var riders = JsonSerializer.Deserialize<IEnumerable<Rider>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(riders);
            Assert.Equal(2, riders.Count());
        }

        [Fact]
        public async Task PutUpdateRider_ReturnsBadRequest_WhenRiderIsNull()
        {
            // Arrange
            var newRider = new { name = "John Doe", phoneNumber = "0701234567" };
            var json = JsonSerializer.Serialize(newRider);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var createResponse = await _client!.PostAsync("/api/riders", createContent);
            var createdRider = JsonSerializer.Deserialize<Rider>(
                await createResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var updateContent = new StringContent("null", System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PutAsync($"/api/riders/{createdRider!.Id}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteRider_DoesNotAffectOtherRiders()
        {
            // Arrange
            var rider1Data = new { name = "John Doe", phoneNumber = "0701234567" };
            var rider2Data = new { name = "Jane Smith", phoneNumber = "0702345678" };

            var create1Response = await _client!.PostAsync("/api/riders",
                new StringContent(JsonSerializer.Serialize(rider1Data),
                    System.Text.Encoding.UTF8, "application/json"));
            var rider1 = JsonSerializer.Deserialize<Rider>(
                await create1Response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var create2Response = await _client!.PostAsync("/api/riders",
                new StringContent(JsonSerializer.Serialize(rider2Data),
                    System.Text.Encoding.UTF8, "application/json"));
            var rider2 = JsonSerializer.Deserialize<Rider>(
                await create2Response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Act
            await _client!.DeleteAsync($"/api/riders/{rider1!.Id}");

            // Assert
            var response = await _client!.GetAsync($"/api/riders/{rider2!.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CachingBehavior_ReturnsConsistentData()
        {
            // Arrange
            var newRider = new { name = "John Doe", phoneNumber = "0701234567" };
            var json = JsonSerializer.Serialize(newRider);
            var createContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await _client!.PostAsync("/api/riders", createContent);

            // Act: Make multiple requests
            var response1 = await _client!.GetAsync("/api/riders");
            var response2 = await _client!.GetAsync("/api/riders");

            // Assert: Both should return same data
            var content1 = await response1.Content.ReadAsStringAsync();
            var content2 = await response2.Content.ReadAsStringAsync();

            var riders1 = JsonSerializer.Deserialize<IEnumerable<Rider>>(content1,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var riders2 = JsonSerializer.Deserialize<IEnumerable<Rider>>(content2,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal(riders1!.Count(), riders2!.Count());
            Assert.Single(riders1);
        }
    }
}
