using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SafeBoda.Api;
using SafeBoda.Core;
using SafeBoda.Infrastructure;
using Xunit;

namespace SafeBoda.Api.Tests
{
    public class TripsControllerIntegrationTests_Comprehensive : IAsyncLifetime
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

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
                    var dbName = $"SafeBoda_TestDb_{Guid.NewGuid()}";
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
        }

        #region GET All Trips Tests

        [Fact]
        public async Task GetAllTrips_ReturnsEmptyList_WhenNoData()
        {
            // Act
            var response = await _client.GetAsync("/api/trips");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var trips = await response.Content.ReadFromJsonAsync<IEnumerable<Trip>>();
            Assert.NotNull(trips);
            Assert.Empty(trips);
        }

        [Fact]
        public async Task GetAllTrips_ReturnsTrips_WhenDataExists()
        {
            // Arrange - Create a trip
            var riderId = Guid.NewGuid();
            var createRequest = new TripRequest(new Location(0, 0), new Location(1, 1), riderId);
            await _client.PostAsJsonAsync("/api/trips", createRequest);

            // Act
            var response = await _client.GetAsync("/api/trips");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var trips = await response.Content.ReadFromJsonAsync<List<Trip>>();
            Assert.NotNull(trips);
            Assert.Single(trips);
        }

        #endregion

        #region GET Trip by ID Tests

        [Fact]
        public async Task GetTripById_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            var tripId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/trips/{tripId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTripById_ReturnsTripData_WhenExists()
        {
            // Arrange - Create a trip
            var riderId = Guid.NewGuid();
            var createRequest = new TripRequest(new Location(0, 0), new Location(1, 1), riderId);
            var createResponse = await _client.PostAsJsonAsync("/api/trips", createRequest);
            var createdTrip = await createResponse.Content.ReadFromJsonAsync<Trip>();

            // Act
            var response = await _client.GetAsync($"/api/trips/{createdTrip.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var trip = await response.Content.ReadFromJsonAsync<Trip>();
            Assert.NotNull(trip);
            Assert.Equal(createdTrip.Id, trip.Id);
        }

        #endregion

        #region POST Create Trip Tests

        [Fact]
        public async Task PostTrip_ReturnsBadRequest_WhenRequestIsNull()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/trips", (TripRequest)null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostTrip_ReturnsCreated_WithValidRequest()
        {
            // Arrange
            var riderId = Guid.NewGuid();
            var request = new TripRequest(new Location(0, 0), new Location(1, 1), riderId);

            // Act
            var response = await _client.PostAsJsonAsync("/api/trips", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var trip = await response.Content.ReadFromJsonAsync<Trip>();
            Assert.NotNull(trip);
            Assert.Equal(riderId, trip.RiderId);
            Assert.Equal(new Location(0, 0), trip.Start);
            Assert.Equal(new Location(1, 1), trip.End);
        }

        [Fact]
        public async Task PostTrip_ThenGet_ReturnsCreatedAndListed()
        {
            // Arrange
            var riderId = Guid.NewGuid();
            var request = new TripRequest(new Location(10, 20), new Location(30, 40), riderId);

            // Act - Create
            var postResponse = await _client.PostAsJsonAsync("/api/trips", request);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

            // Act - Get all
            var getResponse = await _client.GetAsync("/api/trips");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var trips = await getResponse.Content.ReadFromJsonAsync<List<Trip>>();

            // Assert
            Assert.NotNull(trips);
            Assert.Single(trips);
            Assert.Equal(riderId, trips[0].RiderId);
        }

        #endregion

        #region PUT Update Trip Tests

        [Fact]
        public async Task PutTrip_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var tripId = Guid.NewGuid();
            var otherId = Guid.NewGuid();
            var trip = new Trip
            {
                Id = otherId,
                RiderId = Guid.NewGuid(),
                DriverId = Guid.NewGuid(),
                Start = new Location(0, 0),
                End = new Location(1, 1),
                Fare = 100,
                RequestTime = DateTime.UtcNow
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/trips/{tripId}", trip);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutTrip_ReturnsNoContent_WhenValid()
        {
            // Arrange - Create a trip first
            var riderId = Guid.NewGuid();
            var createRequest = new TripRequest(new Location(0, 0), new Location(1, 1), riderId);
            var createResponse = await _client.PostAsJsonAsync("/api/trips", createRequest);
            var createdTrip = await createResponse.Content.ReadFromJsonAsync<Trip>();

            // Prepare update
            var updatedTrip = new Trip
            {
                Id = createdTrip.Id,
                RiderId = createdTrip.RiderId,
                DriverId = createdTrip.DriverId,
                Start = new Location(5, 5),
                End = new Location(6, 6),
                Fare = 200,
                RequestTime = createdTrip.RequestTime
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/trips/{createdTrip.Id}", updatedTrip);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify update
            var getResponse = await _client.GetAsync($"/api/trips/{createdTrip.Id}");
            var updatedTripData = await getResponse.Content.ReadFromJsonAsync<Trip>();
            Assert.Equal(200, updatedTripData.Fare);
            Assert.Equal(new Location(5, 5), updatedTripData.Start);
        }

        #endregion

        #region DELETE Trip Tests

        [Fact]
        public async Task DeleteTrip_ReturnsNoContent()
        {
            // Arrange - Create a trip first
            var riderId = Guid.NewGuid();
            var createRequest = new TripRequest(new Location(0, 0), new Location(1, 1), riderId);
            var createResponse = await _client.PostAsJsonAsync("/api/trips", createRequest);
            var createdTrip = await createResponse.Content.ReadFromJsonAsync<Trip>();

            // Act
            var response = await _client.DeleteAsync($"/api/trips/{createdTrip.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/trips/{createdTrip.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteTrip_RemovesFromList()
        {
            // Arrange - Create two trips
            var riderId = Guid.NewGuid();
            var request1 = new TripRequest(new Location(0, 0), new Location(1, 1), riderId);
            var request2 = new TripRequest(new Location(2, 2), new Location(3, 3), riderId);

            var create1 = await _client.PostAsJsonAsync("/api/trips", request1);
            var create2 = await _client.PostAsJsonAsync("/api/trips", request2);

            var trip1 = await create1.Content.ReadFromJsonAsync<Trip>();
            var trip2 = await create2.Content.ReadFromJsonAsync<Trip>();

            // Act - Delete first trip
            var deleteResponse = await _client.DeleteAsync($"/api/trips/{trip1.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Assert - Only one trip remains
            var getResponse = await _client.GetAsync("/api/trips");
            var trips = await getResponse.Content.ReadFromJsonAsync<List<Trip>>();
            Assert.Single(trips);
            Assert.Equal(trip2.Id, trips[0].Id);
        }

        #endregion

        #region Cache Tests

        [Fact]
        public async Task GetAllTrips_ReturnsCachedResults()
        {
            // Arrange - Create a trip
            var riderId = Guid.NewGuid();
            var request = new TripRequest(new Location(0, 0), new Location(1, 1), riderId);
            await _client.PostAsJsonAsync("/api/trips", request);

            // Act - First call
            var response1 = await _client.GetAsync("/api/trips");
            var trips1 = await response1.Content.ReadFromJsonAsync<List<Trip>>();

            // Act - Second call (should be cached)
            var response2 = await _client.GetAsync("/api/trips");
            var trips2 = await response2.Content.ReadFromJsonAsync<List<Trip>>();

            // Assert - Both return same data
            Assert.Equal(trips1.Count, trips2.Count);
            Assert.Equal(trips1[0].Id, trips2[0].Id);
        }

        [Fact]
        public async Task CacheInvalidation_AfterCreate()
        {
            // Arrange
            var riderId = Guid.NewGuid();
            var request1 = new TripRequest(new Location(0, 0), new Location(1, 1), riderId);

            // Act - Create and cache
            await _client.PostAsJsonAsync("/api/trips", request1);
            var getResponse1 = await _client.GetAsync("/api/trips");
            var trips1 = await getResponse1.Content.ReadFromJsonAsync<List<Trip>>();

            // Act - Create another trip (should invalidate cache)
            var request2 = new TripRequest(new Location(2, 2), new Location(3, 3), riderId);
            await _client.PostAsJsonAsync("/api/trips", request2);

            var getResponse2 = await _client.GetAsync("/api/trips");
            var trips2 = await getResponse2.Content.ReadFromJsonAsync<List<Trip>>();

            // Assert - Second call has more trips (cache was invalidated)
            Assert.Single(trips1);
            Assert.Equal(2, trips2.Count);
        }

        #endregion
    }
}
