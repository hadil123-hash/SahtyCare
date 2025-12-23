using System;
using Microsoft.AspNetCore.Authorization;
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
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public DoctorsController(
            IDoctorRepository repository,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _repository = repository;
            _userManager = userManager;
            _db = db;
        }

        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor},{AppRoles.Patient},{AppRoles.Pharmacist}")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _repository.GetAllAsync();
            var dtos = doctors.Select(d => new DoctorDto
            {
                Id = d.Id,
                FullName = d.FullName,
                Speciality = d.Speciality,
                Email = d.Email
            });
            return Ok(dtos);
        }

        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor},{AppRoles.Patient},{AppRoles.Pharmacist}")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _repository.GetByIdAsync(id);
            if (doctor == null) return NotFound();
            return Ok(new DoctorDto
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                Speciality = doctor.Speciality,
                Email = doctor.Email
            });
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost]
        public IActionResult Create([FromBody] DoctorCreateDto dto)
            => StatusCode(403, "Doctor creation is disabled. Create account via registration then assign role.");

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DoctorUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.Id) return BadRequest();

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var emailInUse = await _db.Doctors.AnyAsync(d => d.Email == dto.Email && d.Id != id);
            if (emailInUse)
                return Conflict("Email already used by another doctor.");

            var doctor = new Doctor
            {
                Id = dto.Id,
                FullName = dto.FullName,
                Speciality = dto.Speciality,
                Email = dto.Email
            };

            var updated = await _repository.UpdateAsync(doctor);
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

                if (!await _userManager.IsInRoleAsync(user, AppRoles.Doctor))
                    await _userManager.AddToRoleAsync(user, AppRoles.Doctor);
            }

            var resultDto = new DoctorDto
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                Speciality = doctor.Speciality,
                Email = doctor.Email
            };

            return Ok(resultDto);
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var hasAppointments = await _db.Appointments.AnyAsync(a => a.DoctorId == id);
            var hasPrescriptions = await _db.Prescriptions.AnyAsync(p => p.DoctorId == id);
            if (hasAppointments || hasPrescriptions)
                return Conflict("Doctor has appointments or prescriptions.");

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

