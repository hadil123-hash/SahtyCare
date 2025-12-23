using Microsoft.EntityFrameworkCore;
using Sahty.API.Data;
using Sahty.API.Repositories.Interfaces;
using Sahty.Metiers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sahty.API.Repositories.Implementations
{
    public class PharmacistRepository : IPharmacistRepository
    {
        private readonly ApplicationDbContext _context; // <-- corrigé ici

        public PharmacistRepository(ApplicationDbContext context) // <-- corrigé ici
        {
            _context = context;
        }

        public async Task<Pharmacist> CreateAsync(Pharmacist pharmacist)
        {
            _context.Pharmacists.Add(pharmacist);
            await _context.SaveChangesAsync();
            return pharmacist;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pharmacist = await _context.Pharmacists.FindAsync(id);
            if (pharmacist == null) return false;

            _context.Pharmacists.Remove(pharmacist);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Pharmacist>> GetAllAsync()
        {
            return await _context.Pharmacists.ToListAsync();
        }

        public async Task<Pharmacist> GetByIdAsync(int id)
        {
            return await _context.Pharmacists.FindAsync(id);
        }

        public async Task<Pharmacist> UpdateAsync(Pharmacist pharmacist)
        {
            var existing = await _context.Pharmacists.FindAsync(pharmacist.Id);
            if (existing == null) return null;

            existing.FullName = pharmacist.FullName;
            existing.Email = pharmacist.Email;
            existing.PharmacyName = pharmacist.PharmacyName;

            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
