using Sahty.Metiers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sahty.API.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetAllAsync();
        Task<Patient> GetByIdAsync(int id);
        Task<Patient> CreateAsync(Patient patient);
        Task<Patient> UpdateAsync(Patient patient);
        Task<bool> DeleteAsync(int id);
    }
}
