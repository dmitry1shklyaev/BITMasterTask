using System.Collections.Generic;
using System.Threading.Tasks;
using BITMasterTask.Models;

namespace BITMasterTask.Data;

public interface ICrewRepository
{
    Task<IReadOnlyList<Crew>> GetAllAsync();
    Task<int> AddAsync(Crew crew);
    Task UpdateAsync(Crew crew);
    Task DeleteAsync(int id);
    Task DeleteInactiveAsync();
}

