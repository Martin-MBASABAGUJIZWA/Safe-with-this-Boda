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
    public class RidersController : ControllerBase
    {
        private readonly IRiderRepository _riderRepository;
        private readonly IMemoryCache _cache;
        private const string RidersCacheKey = "riders_all";

        public RidersController(IRiderRepository riderRepository, IMemoryCache cache)
        {
            _riderRepository = riderRepository ?? throw new ArgumentNullException(nameof(riderRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        // GET: api/riders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rider>>> GetAllRiders()
        {
            // Try to get cached riders
            if (_cache.TryGetValue(RidersCacheKey, out var cachedObj))
            {
                if (cachedObj is IEnumerable<Rider> cached)
                {
                    return Ok(cached);
                }
            }

            var riders = await _riderRepository.GetAllAsync();
            if (riders == null)
            {
                riders = Enumerable.Empty<Rider>();
            }

            // Cache the riders for 1 minute
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };

            _cache.Set(RidersCacheKey, (object)riders, cacheEntryOptions);

            return Ok(riders);
        }

        // GET: api/riders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Rider>> GetRiderById(Guid id)
        {
            var rider = await _riderRepository.GetByIdAsync(id);
            if (rider == null) return NotFound();
            return Ok(rider);
        }

        // POST: api/riders
        [HttpPost]
        public async Task<ActionResult<Rider>> CreateRider([FromBody] Rider rider)
        {
            if (rider == null) return BadRequest();

            var newRider = new Rider(Guid.NewGuid(), rider.Name, rider.PhoneNumber);
            await _riderRepository.AddAsync(newRider);

            // Invalidate cache
            _cache.Remove(RidersCacheKey);

            return CreatedAtAction(nameof(GetRiderById), new { id = newRider.Id }, newRider);
        }

        // PUT: api/riders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRider(Guid id, [FromBody] Rider updatedRider)
        {
            if (updatedRider == null) return BadRequest();
            if (id != updatedRider.Id) return BadRequest("Rider ID mismatch");

            await _riderRepository.UpdateAsync(updatedRider);

            // Invalidate cache
            _cache.Remove(RidersCacheKey);

            return NoContent();
        }

        // DELETE: api/riders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRider(Guid id)
        {
            await _riderRepository.DeleteAsync(id);

            // Invalidate cache
            _cache.Remove(RidersCacheKey);

            return NoContent();
        }
    }
}
