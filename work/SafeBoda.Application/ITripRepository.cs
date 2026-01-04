using System.Collections.Generic;
using System.Threading.Tasks;
using SafeBoda.Core;

namespace SafeBoda.Application;

public interface ITripRepository
{
    Task<Trip?> GetByIdAsync(Guid id);
    Task<IEnumerable<Trip>> GetAllAsync();
    Task<Trip> AddAsync(Trip trip);
    Task UpdateAsync(Trip trip);
    Task DeleteAsync(Guid id);
}

