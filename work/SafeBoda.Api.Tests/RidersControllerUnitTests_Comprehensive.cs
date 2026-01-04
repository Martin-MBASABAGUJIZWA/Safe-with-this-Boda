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
    public class RidersControllerUnitTests_Comprehensive
    {
        private readonly Mock<IRiderRepository> _mockRiderRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly RidersController _controller;

        public RidersControllerUnitTests_Comprehensive()
        {
            _mockRiderRepository = new Mock<IRiderRepository>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _controller = new RidersController(_mockRiderRepository.Object, _memoryCache);
        }

        // GetAllRiders Tests
        [Fact]
        public async Task GetAllRiders_ReturnsOk_WithEmptyList()
        {
            // Arrange
            var riders = new List<Rider>();
            _mockRiderRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(riders);

            // Act
            var result = await _controller.GetAllRiders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
            var returnedRiders = Assert.IsAssignableFrom<IEnumerable<Rider>>(okResult.Value);
            Assert.Empty(returnedRiders);
        }

        [Fact]
        public async Task GetAllRiders_ReturnsOk_WithRidersList()
        {
            // Arrange
            var riders = new List<Rider>
            {
                new Rider(Guid.Parse("11111111-1111-1111-1111-111111111111"), "John Doe", "0701234567"),
                new Rider(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Jane Smith", "0702345678")
            };
            _mockRiderRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(riders);

            // Act
            var result = await _controller.GetAllRiders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRiders = Assert.IsAssignableFrom<IEnumerable<Rider>>(okResult.Value);
            Assert.Equal(2, returnedRiders.Count());
        }

        [Fact]
        public async Task GetAllRiders_UsesCaching_OnSecondCall()
        {
            // Arrange
            var riders = new List<Rider>
            {
                new Rider(Guid.Parse("11111111-1111-1111-1111-111111111111"), "John Doe", "0701234567")
            };
            _mockRiderRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(riders);

            // Act - First call should hit the repository
            var result1 = await _controller.GetAllRiders();
            var okResult1 = Assert.IsType<OkObjectResult>(result1.Result);
            var riders1 = Assert.IsAssignableFrom<IEnumerable<Rider>>(okResult1.Value);

            // Second call should use cached data
            var result2 = await _controller.GetAllRiders();
            var okResult2 = Assert.IsType<OkObjectResult>(result2.Result);
            var riders2 = Assert.IsAssignableFrom<IEnumerable<Rider>>(okResult2.Value);

            // Assert - Verify repository was only called once despite two controller calls
            Assert.Equal(riders1, riders2);
            _mockRiderRepository.Verify(repo => repo.GetAllAsync(), Times.Once());
        }

        // GetRiderById Tests
        [Fact]
        public async Task GetRiderById_ReturnsOk_WithValidId()
        {
            // Arrange
            var riderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var rider = new Rider(riderId, "John Doe", "0701234567");
            _mockRiderRepository.Setup(repo => repo.GetByIdAsync(riderId)).ReturnsAsync(rider);

            // Act
            var result = await _controller.GetRiderById(riderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRider = Assert.IsType<Rider>(okResult.Value);
            Assert.Equal(riderId, returnedRider.Id);
            Assert.Equal("John Doe", returnedRider.Name);
        }

        [Fact]
        public async Task GetRiderById_ReturnsNotFound_WithInvalidId()
        {
            // Arrange
            var riderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            _mockRiderRepository.Setup(repo => repo.GetByIdAsync(riderId)).ReturnsAsync((Rider?)null);

            // Act
            var result = await _controller.GetRiderById(riderId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        // CreateRider Tests
        [Fact]
        public async Task CreateRider_ReturnsCreatedAtAction_WithValidRider()
        {
            // Arrange
            var newRider = new Rider(Guid.NewGuid(), "John Doe", "0701234567");
            _mockRiderRepository.Setup(repo => repo.AddAsync(It.IsAny<Rider>())).ReturnsAsync(newRider);

            // Act
            var result = await _controller.CreateRider(newRider);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(RidersController.GetRiderById), createdResult.ActionName);
            var returnedRider = Assert.IsType<Rider>(createdResult.Value);
            Assert.Equal(newRider.Name, returnedRider.Name);
            _mockRiderRepository.Verify(repo => repo.AddAsync(It.IsAny<Rider>()), Times.Once);
        }

        [Fact]
        public async Task CreateRider_ReturnsBadRequest_WhenRiderIsNull()
        {
            // Act
            var result = await _controller.CreateRider(null!);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
            _mockRiderRepository.Verify(repo => repo.AddAsync(It.IsAny<Rider>()), Times.Never);
        }

        // UpdateRider Tests
        [Fact]
        public async Task UpdateRider_ReturnsNoContent_WithValidRider()
        {
            // Arrange
            var riderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var updatedRider = new Rider(riderId, "John Updated", "0701234567");
            _mockRiderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Rider>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateRider(riderId, updatedRider);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRiderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Rider>()), Times.Once);
        }

        [Fact]
        public async Task UpdateRider_ReturnsBadRequest_WhenRiderIsNull()
        {
            // Arrange
            var riderId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            // Act
            var result = await _controller.UpdateRider(riderId, null!);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _mockRiderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Rider>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRider_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var riderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var differentRider = new Rider(Guid.Parse("22222222-2222-2222-2222-222222222222"), "John Doe", "0701234567");

            // Act
            var result = await _controller.UpdateRider(riderId, differentRider);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockRiderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Rider>()), Times.Never);
        }

        // DeleteRider Tests
        [Fact]
        public async Task DeleteRider_ReturnsNoContent()
        {
            // Arrange
            var riderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            _mockRiderRepository.Setup(repo => repo.DeleteAsync(riderId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteRider(riderId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRiderRepository.Verify(repo => repo.DeleteAsync(riderId), Times.Once);
        }

        [Fact]
        public async Task DeleteRider_InvalidatesCaching()
        {
            // Arrange
            var riderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var existingRiders = new List<Rider>
            {
                new Rider(riderId, "John Doe", "0701234567")
            };
            _mockRiderRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(existingRiders);
            _mockRiderRepository.Setup(repo => repo.DeleteAsync(riderId)).Returns(Task.CompletedTask);

            // Cache the existing riders
            await _controller.GetAllRiders();

            // Act: Delete rider
            await _controller.DeleteRider(riderId);

            // Assert: Cache should be invalidated
            _mockRiderRepository.Reset();
            _mockRiderRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Rider>());
            
            var result = await _controller.GetAllRiders();
            _mockRiderRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }
    }
}
