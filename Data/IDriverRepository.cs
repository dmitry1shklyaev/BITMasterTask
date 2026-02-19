using System.Collections.Generic;
using System.Threading.Tasks;
using BITMasterTask.Models;

namespace BITMasterTask.Data;

public interface IDriverRepository
{
    Task<IReadOnlyList<Driver>> GetAllAsync();
    Task<Driver?> GetByIdAsync(int id);
    Task<int> AddAsync(Driver driver);
    Task UpdateAsync(Driver driver);
    Task DeleteAsync(int id);
}

