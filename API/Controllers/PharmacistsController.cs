using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sahty.API.Data;
using Sahty.API.DTOs;
using Sahty.API.Repositories.Interfaces;
using Sahty.Metiers;
using Sahty.Shared.Auth;
using Sahty.Shared.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Sahty.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PharmacistsController : ControllerBase
    {
        private readonly IPharmacistRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public PharmacistsController(
            IPharmacistRepository repository,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _repository = repository;
            _userManager = userManager;
            _db = db;
        }

        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor}")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var pharmacists = await _repository.GetAllAsync();
            var dtos = pharmacists.Select(p => new PharmacistDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                PharmacyName = p.PharmacyName
            });
            return Ok(dtos);
        }

        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor}")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pharmacist = await _repository.GetByIdAsync(id);
            if (pharmacist == null) return NotFound();
            return Ok(new PharmacistDto
            {
                Id = pharmacist.Id,
                FullName = pharmacist.FullName,
                Email = pharmacist.Email,
                PharmacyName = pharmacist.PharmacyName
            });
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost]
        public IActionResult Create([FromBody] PharmacistCreateDto dto)
            => StatusCode(403, "Pharmacist creation is disabled. Use role assignment instead.");

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PharmacistUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.Id) return BadRequest();

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var emailInUse = await _db.Pharmacists.AnyAsync(p => p.Email == dto.Email && p.Id != id);
            if (emailInUse)
                return Conflict("Email already used by another pharmacist.");

            var pharmacist = new Pharmacist
            {
                Id = dto.Id,
                FullName = dto.FullName,
                Email = dto.Email,
                PharmacyName = dto.PharmacyName
            };

            var updated = await _repository.UpdateAsync(pharmacist);
            if (updated == null) return NotFound();

            var user = await _userManager.FindByEmailAsync(existing.Email);
            if (user is null && !string.Equals(existing.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
                user = await _userManager.FindByEmailAsync(dto.Email);

            if (user != null)
            {
                user.Email = dto.Email;
                user.UserName = dto.Email;
                user.FullName = dto.FullName;
                await _userManager.UpdateAsync(user);

                if (!await _userManager.IsInRoleAsync(user, AppRoles.Pharmacist))
                    await _userManager.AddToRoleAsync(user, AppRoles.Pharmacist);
            }

            var resultDto = new PharmacistDto
            {
                Id = pharmacist.Id,
                FullName = pharmacist.FullName,
                Email = pharmacist.Email,
                PharmacyName = pharmacist.PharmacyName
            };

            return Ok(resultDto);
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var hasPrescriptions = await _db.Prescriptions.AnyAsync(p => p.PharmacistId == id);
            if (hasPrescriptions)
                return Conflict("Pharmacist has prescriptions.");

            var deleted = await _repository.DeleteAsync(id);
            if (!deleted) return NotFound();

            if (!string.IsNullOrWhiteSpace(existing.Email))
            {
                var user = await _userManager.FindByEmailAsync(existing.Email);
                if (user != null)
                    await _userManager.DeleteAsync(user);
            }
            return NoContent();
        }
    }
}
