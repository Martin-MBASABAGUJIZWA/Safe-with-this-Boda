using Microsoft.EntityFrameworkCore;
using SafeBoda.Application;
using SafeBoda.Core;

namespace SafeBoda.Infrastructure
{
    public class EfDriverRepository(SafeBodaDbContext db) : IDriverRepository
    {
        public async Task<Driver?> GetByIdAsync(Guid id)
        {
            return await db.Drivers.FindAsync(id);
        }

        public async Task<IEnumerable<Driver>> GetAllAsync()
        {
            return await db.Drivers.ToListAsync();
        }

        public async Task<Driver> AddAsync(Driver driver)
        {
            await db.Drivers.AddAsync(driver);
            await db.SaveChangesAsync();
            return driver;
        }

        public async Task UpdateAsync(Driver driver)
        {
            db.Drivers.Update(driver);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var driver = await db.Drivers.FindAsync(id);
            if (driver != null)
            {
                db.Drivers.Remove(driver);
                await db.SaveChangesAsync();
            }
        }
    }
}
