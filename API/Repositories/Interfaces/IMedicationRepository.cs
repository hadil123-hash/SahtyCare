using Sahty.Metiers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sahty.API.Repositories.Interfaces
{
    public interface IMedicationRepository
    {
        Task<IEnumerable<Medication>> GetAllAsync();
        Task<Medication> GetByIdAsync(int id);
        Task<Medication> CreateAsync(Medication medication);
        Task<Medication> UpdateAsync(Medication medication);
        Task<bool> DeleteAsync(int id);
    }
}
