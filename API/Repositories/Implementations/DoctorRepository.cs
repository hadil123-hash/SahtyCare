using Microsoft.EntityFrameworkCore;
using Sahty.API.Data;
using Sahty.API.Repositories.Interfaces;
using Sahty.Metiers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sahty.API.Repositories.Implementations
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Doctor> CreateAsync(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return doctor;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return false;

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            return await _context.Doctors.ToListAsync();
        }

        public async Task<Doctor> GetByIdAsync(int id)
        {
            return await _context.Doctors.FindAsync(id);
        }

        public async Task<Doctor> UpdateAsync(Doctor doctor)
        {
            var existing = await _context.Doctors.FindAsync(doctor.Id);
            if (existing == null) return null;

            existing.FullName = doctor.FullName;
            existing.Speciality = doctor.Speciality;
            existing.Email = doctor.Email;

            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
