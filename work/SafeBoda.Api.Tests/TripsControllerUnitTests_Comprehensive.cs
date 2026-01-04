using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SafeBoda.Api.Controllers;
using SafeBoda.Application;
using SafeBoda.Core;
using Xunit;
using Microsoft.Extensions.Caching.Memory;

namespace SafeBoda.Api.Tests
{
    public class TripsControllerUnitTests_Comprehensive
    {
        private IMemoryCache CreateMockCache()
        {
            return new MemoryCache(new MemoryCacheOptions());
        }

        #region GET All Trips Tests

        [Fact]
        public async Task GetAllTrips_ReturnsOk_WithTrips()
        {
            // Arrange
            var mockRepo = new Mock<ITripRepository>();
            var trips = new List<Trip>
            {
                new Trip { Id = Guid.NewGuid(), RiderId = Guid.NewGuid(), DriverId = Guid.NewGuid(), Start = new Location(0,0), End = new Location(1,1), Fare = 100, RequestTime = DateTime.UtcNow }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(trips);
            var mockCache = CreateMockCache();

            var controller = new TripsController(mockRepo.Object, mockCache);

            // Act
            var result = await controller.GetAllTrips();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<Trip>>(ok.Value);
            Assert.NotEmpty(returned);
        }

        [Fact]
        public async Task GetAllTrips_ReturnsOk_WithEmptyList()
        {
            // Arrange
            var mockRepo = new Mock<ITripRepository>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Trip>());
            var mockCache = CreateMockCache();

            var controller = new TripsController(mockRepo.Object, mockCache);

            // Act
            var result = await controller.GetAllTrips();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<Trip>>(ok.Value);
            Assert.Empty(returned);
        }

        [Fact]
        public async Task GetAllTrips_UsesCachedData_OnSecondCall()
        {
            // Arrange
            var mockRepo = new Mock<ITripRepository>();
            var trips = new List<Trip>
            {
                new Trip { Id = Guid.NewGuid(), RiderId = Guid.NewGuid(), DriverId = Guid.NewGuid(), Start = new Location(0,0), End = new Location(1,1), Fare = 100, RequestTime = DateTime.UtcNow }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(trips);
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Act - First call
            await controller.GetAllTrips();
            
            // Act - Second call
            await controller.GetAllTrips();

            // Assert - Repository called only once (cache used on second call)
            mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        #endregion

        #region GET Trip by ID Tests

        [Fact]
        public async Task GetTripById_ReturnsOk_WhenFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var trip = new Trip { Id = id, RiderId = Guid.NewGuid(), DriverId = Guid.NewGuid(), Start = new Location(0, 0), End = new Location(1, 1), Fare = 100, RequestTime = DateTime.UtcNow };
            var mockRepo = new Mock<ITripRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(trip);
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Act
            var result = await controller.GetTripById(id);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<Trip>(ok.Value);
            Assert.Equal(id, returned.Id);
        }

        [Fact]
        public async Task GetTripById_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            var id = Guid.NewGuid();
            var mockRepo = new Mock<ITripRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Trip?)null);
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Act
            var result = await controller.GetTripById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region POST Create Trip Tests

        [Fact]
        public async Task CreateTrip_ReturnsBadRequest_WhenRequestIsNull()
        {
            // Arrange
            var mockRepo = new Mock<ITripRepository>();
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Act
            var result = await controller.CreateTrip(null);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task CreateTrip_ReturnsCreated_WithValidRequest()
        {
            // Arrange
            var riderId = Guid.NewGuid();
            var request = new TripRequest(new Location(0, 0), new Location(1, 1), riderId);

            var mockRepo = new Mock<ITripRepository>();
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Trip>())).ReturnsAsync((Trip t) => t);
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Act
            var result = await controller.CreateTrip(request);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(controller.GetTripById), created.ActionName);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Trip>()), Times.Once);
        }

        [Fact]
        public async Task CreateTrip_InvalidatesCacheAfterAdd()
        {
            // Arrange
            var riderId = Guid.NewGuid();
            var request = new TripRequest(new Location(0, 0), new Location(1, 1), riderId);

            var mockRepo = new Mock<ITripRepository>();
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Trip>())).ReturnsAsync((Trip t) => t);
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Prime cache
            mockCache.Set("trips_all", new List<Trip>());

            // Act
            await controller.CreateTrip(request);

            // Assert - cache should be removed
            object cachedTrips;
            Assert.False(mockCache.TryGetValue("trips_all", out cachedTrips));
        }

        #endregion

        #region PUT Update Trip Tests

        [Fact]
        public async Task UpdateTrip_ReturnsBadRequest_WhenTripIsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            var mockRepo = new Mock<ITripRepository>();
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Act
            var result = await controller.UpdateTrip(id, null);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateTrip_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var id = Guid.NewGuid();
            var otherId = Guid.NewGuid();
            var trip = new Trip { Id = otherId, RiderId = Guid.NewGuid(), DriverId = Guid.NewGuid(), Start = new Location(0, 0), End = new Location(1, 1), Fare = 100, RequestTime = DateTime.UtcNow };

            var mockRepo = new Mock<ITripRepository>();
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Act
            var result = await controller.UpdateTrip(id, trip);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("ID mismatch", badRequest.Value.ToString());
        }

        [Fact]
        public async Task UpdateTrip_ReturnsNoContent_WithValidData()
        {
            // Arrange
            var id = Guid.NewGuid();
            var trip = new Trip { Id = id, RiderId = Guid.NewGuid(), DriverId = Guid.NewGuid(), Start = new Location(0, 0), End = new Location(1, 1), Fare = 100, RequestTime = DateTime.UtcNow };

            var mockRepo = new Mock<ITripRepository>();
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Trip>())).Returns(Task.CompletedTask);
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Act
            var result = await controller.UpdateTrip(id, trip);

            // Assert
            Assert.IsType<NoContentResult>(result);
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Trip>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTrip_InvalidatesCacheAfterUpdate()
        {
            // Arrange
            var id = Guid.NewGuid();
            var trip = new Trip { Id = id, RiderId = Guid.NewGuid(), DriverId = Guid.NewGuid(), Start = new Location(0, 0), End = new Location(1, 1), Fare = 100, RequestTime = DateTime.UtcNow };

            var mockRepo = new Mock<ITripRepository>();
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Trip>())).Returns(Task.CompletedTask);
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Prime cache
            mockCache.Set("trips_all", new List<Trip>());

            // Act
            await controller.UpdateTrip(id, trip);

            // Assert - cache should be removed
            object cachedTrips;
            Assert.False(mockCache.TryGetValue("trips_all", out cachedTrips));
        }

        #endregion

        #region DELETE Trip Tests

        [Fact]
        public async Task DeleteTrip_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var mockRepo = new Mock<ITripRepository>();
            mockRepo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Act
            var result = await controller.DeleteTrip(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            mockRepo.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteTrip_InvalidatesCacheAfterDelete()
        {
            // Arrange
            var id = Guid.NewGuid();
            var mockRepo = new Mock<ITripRepository>();
            mockRepo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);
            var mockCache = CreateMockCache();
            var controller = new TripsController(mockRepo.Object, mockCache);

            // Prime cache
            mockCache.Set("trips_all", new List<Trip>());

            // Act
            await controller.DeleteTrip(id);

            // Assert - cache should be removed
            object cachedTrips;
            Assert.False(mockCache.TryGetValue("trips_all", out cachedTrips));
        }

        #endregion
    }
}
