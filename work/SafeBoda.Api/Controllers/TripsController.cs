using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SafeBoda.Application;
using SafeBoda.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;


namespace SafeBoda.Api.Controllers
{


    // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly ITripRepository _tripRepository;
        private readonly IMemoryCache _cache;
        private const string TripsCacheKey = "trips_all";

        public TripsController(ITripRepository tripRepository, IMemoryCache cache)
        {
            _tripRepository = tripRepository;
            _cache = cache;
        }

        // GET: api/trips
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trip>>> GetAllTrips()
        {
            // Try to get cached trips (use non-generic TryGetValue)
            object cachedObj;
            if (_cache.TryGetValue(TripsCacheKey, out cachedObj))
            {
                if (cachedObj is IEnumerable<Trip> cached)
                {
                    return Ok(cached);
                }
            }

            var trips = await _tripRepository.GetAllAsync();
            if (trips == null)
            {
                trips = Enumerable.Empty<Trip>();
            }

            // Cache the trips for 1 minute to improve responsiveness
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };

            // Use non-generic Set method
            _cache.Set(TripsCacheKey, (object)trips, cacheEntryOptions);

            return Ok(trips);
        }

        // GET: api/trips/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Trip>> GetTripById(Guid id)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip == null) return NotFound();
            return Ok(trip);
        }

        // POST: api/trips
        [HttpPost]
        public async Task<ActionResult<Trip>> CreateTrip([FromBody] TripRequest request)
        {
            if (request == null) return BadRequest();

            var newTrip = new Trip
            {
                Id = Guid.NewGuid(),
                RiderId = request.RiderId,
                DriverId = Guid.NewGuid(), // Simulated assignment
                Start = request.StartLocation,
                End = request.EndLocation,
                Fare = 2500m,
                RequestTime = DateTime.UtcNow
            };

            await _tripRepository.AddAsync(newTrip);

            // Invalidate cache after creating a trip
            _cache.Remove(TripsCacheKey);

            return CreatedAtAction(nameof(GetTripById), new { id = newTrip.Id }, newTrip);
        }

        // PUT: api/trips/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrip(Guid id, [FromBody] Trip updatedTrip)
        {
            if (updatedTrip == null) return BadRequest();
            if (id != updatedTrip.Id) return BadRequest("Trip ID mismatch");

            await _tripRepository.UpdateAsync(updatedTrip);
            // Invalidate cache after update
            _cache.Remove(TripsCacheKey);

            return NoContent();
        }

        // DELETE: api/trips/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(Guid id)
        {
            await _tripRepository.DeleteAsync(id);
            // Invalidate cache after delete
            _cache.Remove(TripsCacheKey);

            return NoContent();
        }
    }

}