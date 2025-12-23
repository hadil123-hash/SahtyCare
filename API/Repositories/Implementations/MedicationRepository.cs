using Microsoft.EntityFrameworkCore;
using Sahty.Metiers;
using Sahty.API.Data;
using Sahty.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sahty.API.Repositories.Implementations
{
    public class MedicationRepository : IMedicationRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Medication> CreateAsync(Medication medication)
        {
            _context.Medications.Add(medication);
            await _context.SaveChangesAsync();
            return medication;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null) return false;

            _context.Medications.Remove(medication);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Medication>> GetAllAsync()
        {
            return await _context.Medications.ToListAsync();
        }

        public async Task<Medication> GetByIdAsync(int id)
        {
            return await _context.Medications.FindAsync(id);
        }

        public async Task<Medication> UpdateAsync(Medication medication)
        {
            var existing = await _context.Medications.FindAsync(medication.Id);
            if (existing == null) return null;

            existing.Name = medication.Name;
            existing.Stock = medication.Stock;
            existing.Description = medication.Description;

            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
