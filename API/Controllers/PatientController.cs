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
    public class PatientsController : ControllerBase
    {
        private readonly IPatientRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public PatientsController(
            IPatientRepository repository,
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
            var patients = await _repository.GetAllAsync();
            var dtos = patients.Select(p => new PatientDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                DateOfBirth = p.DateOfBirth,
                PhoneNumber = p.PhoneNumber
            });
            return Ok(dtos);
        }

        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor}")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _repository.GetByIdAsync(id);
            if (patient == null) return NotFound();
            return Ok(new PatientDto
            {
                Id = patient.Id,
                FullName = patient.FullName,
                Email = patient.Email,
                DateOfBirth = patient.DateOfBirth,
                PhoneNumber = patient.PhoneNumber
            });
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost]
        public IActionResult Create([FromBody] PatientCreateDto dto)
            => StatusCode(403, "Patient creation is disabled. Use registration then assign role.");

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PatientUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.Id) return BadRequest();

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var emailInUse = await _db.Patients.AnyAsync(p => p.Email == dto.Email && p.Id != id);
            if (emailInUse)
                return Conflict("Email already used by another patient.");

            var patient = new Patient
            {
                Id = dto.Id,
                FullName = dto.FullName,
                Email = dto.Email,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber
            };

            var updated = await _repository.UpdateAsync(patient);
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

                if (!await _userManager.IsInRoleAsync(user, AppRoles.Patient))
                    await _userManager.AddToRoleAsync(user, AppRoles.Patient);
            }

            var resultDto = new PatientDto
            {
                Id = patient.Id,
                FullName = patient.FullName,
                Email = patient.Email,
                DateOfBirth = patient.DateOfBirth,
                PhoneNumber = patient.PhoneNumber
            };

            return Ok(resultDto);
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var hasAppointments = await _db.Appointments.AnyAsync(a => a.PatientId == id);
            var hasPrescriptions = await _db.Prescriptions.AnyAsync(p => p.PatientId == id);
            if (hasAppointments || hasPrescriptions)
                return Conflict("Patient has appointments or prescriptions.");

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
