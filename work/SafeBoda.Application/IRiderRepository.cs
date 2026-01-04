using System.Collections.Generic;
using System.Threading.Tasks;
using SafeBoda.Core;

namespace SafeBoda.Application;

public interface IRiderRepository
{
    Task<Rider?> GetByIdAsync(Guid id);
    Task<IEnumerable<Rider>> GetAllAsync();
    Task<Rider> AddAsync(Rider rider);
    Task UpdateAsync(Rider rider);
    Task DeleteAsync(Guid id);
}
