using Microsoft.EntityFrameworkCore;
using Sahty.API.Data;
using Sahty.API.Repositories.Interfaces;
using Sahty.Metiers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sahty.API.Repositories.Implementations
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;

        public PatientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Patient> CreateAsync(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return false;

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            return await _context.Patients.ToListAsync();
        }

        public async Task<Patient> GetByIdAsync(int id)
        {
            return await _context.Patients.FindAsync(id);
        }

        public async Task<Patient> UpdateAsync(Patient patient)
        {
            var existing = await _context.Patients.FindAsync(patient.Id);
            if (existing == null) return null;

            existing.FullName = patient.FullName;
            existing.Email = patient.Email;
            existing.DateOfBirth = patient.DateOfBirth;
            existing.PhoneNumber = patient.PhoneNumber;

            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
