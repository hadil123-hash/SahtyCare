using Sahty.Metiers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sahty.API.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<Appointment> GetByIdAsync(int id);
        Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
        Task<Appointment> CreateAsync(Appointment appointment);
        Task<Appointment> UpdateStatusAsync(int id, AppointmentStatus status);
    }
}
