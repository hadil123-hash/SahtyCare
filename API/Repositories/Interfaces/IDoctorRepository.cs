using Sahty.Metiers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sahty.API.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<Doctor>> GetAllAsync();
        Task<Doctor> GetByIdAsync(int id);
        Task<Doctor> CreateAsync(Doctor doctor);
        Task<Doctor> UpdateAsync(Doctor doctor);
        Task<bool> DeleteAsync(int id);
    }
}
