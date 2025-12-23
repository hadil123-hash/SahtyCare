using Sahty.Metiers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sahty.API.Repositories.Interfaces
{
    public interface IPharmacistRepository
    {
        Task<IEnumerable<Pharmacist>> GetAllAsync();
        Task<Pharmacist> GetByIdAsync(int id);
        Task<Pharmacist> CreateAsync(Pharmacist pharmacist);
        Task<Pharmacist> UpdateAsync(Pharmacist pharmacist);
        Task<bool> DeleteAsync(int id);
    }
}
