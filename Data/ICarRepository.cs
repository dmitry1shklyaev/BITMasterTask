using System.Collections.Generic;
using System.Threading.Tasks;
using BITMasterTask.Models;

namespace BITMasterTask.Data;

public interface ICarRepository
{
    Task<IReadOnlyList<Car>> GetAllAsync();
    Task<Car?> GetByIdAsync(int id);
    Task<int> AddAsync(Car car);
    Task UpdateAsync(Car car);
    Task DeleteAsync(int id);
}

