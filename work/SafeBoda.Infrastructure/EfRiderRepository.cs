using Microsoft.EntityFrameworkCore;
using SafeBoda.Application;
using SafeBoda.Core;

namespace SafeBoda.Infrastructure
{
    public class EfRiderRepository(SafeBodaDbContext db) : IRiderRepository
    {
        public async Task<Rider?> GetByIdAsync(Guid id)
        {
            return await db.Riders.FindAsync(id);
        }

        public async Task<IEnumerable<Rider>> GetAllAsync()
        {
            return await db.Riders.ToListAsync();
        }

        public async Task<Rider> AddAsync(Rider rider)
        {
            await db.Riders.AddAsync(rider);
            await db.SaveChangesAsync();
            return rider;
        }

        public async Task UpdateAsync(Rider rider)
        {
            db.Riders.Update(rider);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var rider = await db.Riders.FindAsync(id);
            if (rider != null)
            {
                db.Riders.Remove(rider);
                await db.SaveChangesAsync();
            }
        }
    }
}
