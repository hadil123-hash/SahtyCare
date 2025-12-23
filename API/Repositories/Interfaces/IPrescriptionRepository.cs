using Sahty.Metiers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sahty.API.Repositories.Interfaces
{
    public interface IPrescriptionRepository
    {
        Task<IEnumerable<Prescription>> GetAllAsync();
        Task<Prescription?> GetByIdAsync(int id);
        Task<IEnumerable<Prescription>> GetByPatientIdAsync(int patientId);
        Task<IEnumerable<Prescription>> GetByDoctorIdAsync(int doctorId);
        Task<IEnumerable<Prescription>> GetByPharmacistIdAsync(int pharmacistId);
        Task<Prescription> CreateAsync(Prescription prescription);
        Task<bool> UpdateAsync(Prescription prescription);
        Task<bool> DeleteAsync(int id);
    }
}
