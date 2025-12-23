using Microsoft.EntityFrameworkCore;
using Sahty.API.Data;
using Sahty.API.Repositories.Interfaces;
using Sahty.Metiers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sahty.API.Repositories.Implementations
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Prescription>> GetAllAsync() =>
            await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.Pharmacist)
                .Include(p => p.Medication)
                .ToListAsync();

        public async Task<Prescription?> GetByIdAsync(int id) =>
            await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.Pharmacist)
                .Include(p => p.Medication)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Prescription>> GetByPatientIdAsync(int patientId) =>
            await _context.Prescriptions
                .Where(p => p.PatientId == patientId)
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.Pharmacist)
                .Include(p => p.Medication)
                .ToListAsync();

        public async Task<IEnumerable<Prescription>> GetByDoctorIdAsync(int doctorId) =>
            await _context.Prescriptions
                .Where(p => p.DoctorId == doctorId)
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.Pharmacist)
                .Include(p => p.Medication)
                .ToListAsync();

        public async Task<IEnumerable<Prescription>> GetByPharmacistIdAsync(int pharmacistId) =>
            await _context.Prescriptions
                .Where(p => p.PharmacistId == pharmacistId)
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.Pharmacist)
                .Include(p => p.Medication)
                .ToListAsync();

        public async Task<Prescription> CreateAsync(Prescription prescription)
        {
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();
            return prescription;
        }

        public async Task<bool> UpdateAsync(Prescription prescription)
        {
            var exist = await _context.Prescriptions.FindAsync(prescription.Id);
            if (exist == null) return false;

            exist.PatientId = prescription.PatientId;
            exist.DoctorId = prescription.DoctorId;
            exist.PharmacistId = prescription.PharmacistId;
            exist.MedicationId = prescription.MedicationId;
            exist.DateIssued = prescription.DateIssued;
            exist.Dosage = prescription.Dosage;
            exist.Notes = prescription.Notes;
            exist.Status = prescription.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exist = await _context.Prescriptions.FindAsync(id);
            if (exist == null) return false;

            _context.Prescriptions.Remove(exist);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
