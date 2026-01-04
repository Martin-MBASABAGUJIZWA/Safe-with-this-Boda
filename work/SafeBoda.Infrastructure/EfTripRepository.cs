using Microsoft.EntityFrameworkCore;
using SafeBoda.Application;
using SafeBoda.Core;

namespace SafeBoda.Infrastructure
{
    public class EfTripRepository(SafeBodaDbContext db) : ITripRepository
    {
        public async Task<Trip?> GetByIdAsync(Guid id)
        {
            return await db.Trips.FindAsync(id);
        }

        public async Task<IEnumerable<Trip>> GetAllAsync()
        {
            return await db.Trips.ToListAsync();
        }

        public async Task<Trip> AddAsync(Trip trip)
        {
            await db.Trips.AddAsync(trip);
            await db.SaveChangesAsync();
            return trip;
        }

        public async Task UpdateAsync(Trip trip)
        {
            db.Trips.Update(trip);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var trip = await db.Trips.FindAsync(id);
            if (trip != null)
            {
                db.Trips.Remove(trip);
                await db.SaveChangesAsync();
            }
        }
    }
}
