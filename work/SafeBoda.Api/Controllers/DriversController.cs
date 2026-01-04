using Microsoft.AspNetCore.Mvc;
using SafeBoda.Application;
using SafeBoda.Core;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafeBoda.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriversController : ControllerBase
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IMemoryCache _cache;
        private const string DriversCacheKey = "drivers_all";

        public DriversController(IDriverRepository driverRepository, IMemoryCache cache)
        {
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        // GET: api/drivers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Driver>>> GetAllDrivers()
        {
            // Try to get cached drivers
            if (_cache.TryGetValue(DriversCacheKey, out var cachedObj))
            {
                if (cachedObj is IEnumerable<Driver> cached)
                {
                    return Ok(cached);
                }
            }

            var drivers = await _driverRepository.GetAllAsync();
            if (drivers == null)
            {
                drivers = Enumerable.Empty<Driver>();
            }

            // Cache the drivers for 1 minute
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };

            _cache.Set(DriversCacheKey, (object)drivers, cacheEntryOptions);

            return Ok(drivers);
        }

        // GET: api/drivers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Driver>> GetDriverById(Guid id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);
            if (driver == null) return NotFound();
            return Ok(driver);
        }

        // POST: api/drivers
        [HttpPost]
        public async Task<ActionResult<Driver>> CreateDriver([FromBody] Driver driver)
        {
            if (driver == null) return BadRequest();

            var newDriver = new Driver(Guid.NewGuid(), driver.Name, driver.PhoneNumber, driver.MotoPlateNumber);
            await _driverRepository.AddAsync(newDriver);

            // Invalidate cache
            _cache.Remove(DriversCacheKey);

            return CreatedAtAction(nameof(GetDriverById), new { id = newDriver.Id }, newDriver);
        }

        // PUT: api/drivers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDriver(Guid id, [FromBody] Driver updatedDriver)
        {
            if (updatedDriver == null) return BadRequest();
            if (id != updatedDriver.Id) return BadRequest("Driver ID mismatch");

            await _driverRepository.UpdateAsync(updatedDriver);

            // Invalidate cache
            _cache.Remove(DriversCacheKey);

            return NoContent();
        }

        // DELETE: api/drivers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriver(Guid id)
        {
            await _driverRepository.DeleteAsync(id);

            // Invalidate cache
            _cache.Remove(DriversCacheKey);

            return NoContent();
        }
    }
}
