using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using SafeBoda.Api.Controllers;
using SafeBoda.Application;
using SafeBoda.Core;
using Xunit;

namespace SafeBoda.Api.Tests
{
    public class DriversControllerUnitTests_Comprehensive
    {
        private readonly Mock<IDriverRepository> _mockDriverRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly DriversController _controller;

        public DriversControllerUnitTests_Comprehensive()
        {
            _mockDriverRepository = new Mock<IDriverRepository>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _controller = new DriversController(_mockDriverRepository.Object, _memoryCache);
        }

        // GetAllDrivers Tests
        [Fact]
        public async Task GetAllDrivers_ReturnsOk_WithEmptyList()
        {
            // Arrange
            var drivers = new List<Driver>();
            _mockDriverRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(drivers);

            // Act
            var result = await _controller.GetAllDrivers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
            var returnedDrivers = Assert.IsAssignableFrom<IEnumerable<Driver>>(okResult.Value);
            Assert.Empty(returnedDrivers);
        }

        [Fact]
        public async Task GetAllDrivers_ReturnsOk_WithDriversList()
        {
            // Arrange
            var drivers = new List<Driver>
            {
                new Driver(Guid.Parse("11111111-1111-1111-1111-111111111111"), "John Doe", "0701234567", "UBE123"),
                new Driver(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Jane Smith", "0702345678", "UBE456")
            };
            _mockDriverRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(drivers);

            // Act
            var result = await _controller.GetAllDrivers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDrivers = Assert.IsAssignableFrom<IEnumerable<Driver>>(okResult.Value);
            Assert.Equal(2, returnedDrivers.Count());
        }

        [Fact]
        public async Task GetAllDrivers_UsesCaching_OnSecondCall()
        {
            // Arrange
            var drivers = new List<Driver>
            {
                new Driver(Guid.Parse("11111111-1111-1111-1111-111111111111"), "John Doe", "0701234567", "UBE123")
            };
            _mockDriverRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(drivers);

            // Act - First call should hit the repository
            var result1 = await _controller.GetAllDrivers();
            var okResult1 = Assert.IsType<OkObjectResult>(result1.Result);
            var drivers1 = Assert.IsAssignableFrom<IEnumerable<Driver>>(okResult1.Value);

            // Second call should use cached data
            var result2 = await _controller.GetAllDrivers();
            var okResult2 = Assert.IsType<OkObjectResult>(result2.Result);
            var drivers2 = Assert.IsAssignableFrom<IEnumerable<Driver>>(okResult2.Value);

            // Assert - Verify repository was only called once despite two controller calls
            Assert.Equal(drivers1, drivers2);
            _mockDriverRepository.Verify(repo => repo.GetAllAsync(), Times.Once());
        }

        // GetDriverById Tests
        [Fact]
        public async Task GetDriverById_ReturnsOk_WithValidId()
        {
            // Arrange
            var driverId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var driver = new Driver(driverId, "John Doe", "0701234567", "UBE123");
            _mockDriverRepository.Setup(repo => repo.GetByIdAsync(driverId)).ReturnsAsync(driver);

            // Act
            var result = await _controller.GetDriverById(driverId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDriver = Assert.IsType<Driver>(okResult.Value);
            Assert.Equal(driverId, returnedDriver.Id);
            Assert.Equal("John Doe", returnedDriver.Name);
        }

        [Fact]
        public async Task GetDriverById_ReturnsNotFound_WithInvalidId()
        {
            // Arrange
            var driverId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            _mockDriverRepository.Setup(repo => repo.GetByIdAsync(driverId)).ReturnsAsync((Driver?)null);

            // Act
            var result = await _controller.GetDriverById(driverId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        // CreateDriver Tests
        [Fact]
        public async Task CreateDriver_ReturnsCreatedAtAction_WithValidDriver()
        {
            // Arrange
            var newDriver = new Driver(Guid.NewGuid(), "John Doe", "0701234567", "UBE123");
            _mockDriverRepository.Setup(repo => repo.AddAsync(It.IsAny<Driver>())).ReturnsAsync(newDriver);

            // Act
            var result = await _controller.CreateDriver(newDriver);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(DriversController.GetDriverById), createdResult.ActionName);
            var returnedDriver = Assert.IsType<Driver>(createdResult.Value);
            Assert.Equal(newDriver.Name, returnedDriver.Name);
            _mockDriverRepository.Verify(repo => repo.AddAsync(It.IsAny<Driver>()), Times.Once);
        }

        [Fact]
        public async Task CreateDriver_ReturnsBadRequest_WhenDriverIsNull()
        {
            // Act
            var result = await _controller.CreateDriver(null!);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
            _mockDriverRepository.Verify(repo => repo.AddAsync(It.IsAny<Driver>()), Times.Never);
        }

        // UpdateDriver Tests
        [Fact]
        public async Task UpdateDriver_ReturnsNoContent_WithValidDriver()
        {
            // Arrange
            var driverId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var updatedDriver = new Driver(driverId, "John Updated", "0701234567", "UBE123");
            _mockDriverRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Driver>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateDriver(driverId, updatedDriver);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockDriverRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Driver>()), Times.Once);
        }

        [Fact]
        public async Task UpdateDriver_ReturnsBadRequest_WhenDriverIsNull()
        {
            // Arrange
            var driverId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            // Act
            var result = await _controller.UpdateDriver(driverId, null!);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _mockDriverRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Driver>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDriver_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var driverId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var differentDriver = new Driver(Guid.Parse("22222222-2222-2222-2222-222222222222"), "John Doe", "0701234567", "UBE123");

            // Act
            var result = await _controller.UpdateDriver(driverId, differentDriver);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockDriverRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Driver>()), Times.Never);
        }

        // DeleteDriver Tests
        [Fact]
        public async Task DeleteDriver_ReturnsNoContent()
        {
            // Arrange
            var driverId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            _mockDriverRepository.Setup(repo => repo.DeleteAsync(driverId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteDriver(driverId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockDriverRepository.Verify(repo => repo.DeleteAsync(driverId), Times.Once);
        }

        [Fact]
        public async Task DeleteDriver_InvalidatesCaching()
        {
            // Arrange
            var driverId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var existingDrivers = new List<Driver>
            {
                new Driver(driverId, "John Doe", "0701234567", "UBE123")
            };
            _mockDriverRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(existingDrivers);
            _mockDriverRepository.Setup(repo => repo.DeleteAsync(driverId)).Returns(Task.CompletedTask);

            // Cache the existing drivers
            await _controller.GetAllDrivers();

            // Act: Delete driver
            await _controller.DeleteDriver(driverId);

            // Assert: Cache should be invalidated
            _mockDriverRepository.Reset();
            _mockDriverRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Driver>());
            
            var result = await _controller.GetAllDrivers();
            _mockDriverRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }
    }
}
