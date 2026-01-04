using System.Collections.Generic;
using System.Threading.Tasks;
using SafeBoda.Core;

namespace SafeBoda.Application;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(Guid id);
    Task<IEnumerable<Driver>> GetAllAsync();
    Task<Driver> AddAsync(Driver driver);
    Task UpdateAsync(Driver driver);
    Task DeleteAsync(Guid id);
}
