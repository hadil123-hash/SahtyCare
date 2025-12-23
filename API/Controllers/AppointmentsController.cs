using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sahty.API.Data;
using Sahty.API.DTOs;
using Sahty.API.Repositories.Interfaces;
using Sahty.Metiers;
using Sahty.Shared.Auth;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sahty.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentRepository _repository;
        private readonly ApplicationDbContext _db;

        public AppointmentsController(IAppointmentRepository repository, ApplicationDbContext db)
        {
            _repository = repository;
            _db = db;
        }

        // ---------------- GET ALL (Admin + Doctor) ----------------
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor}")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<Appointment> appointments;

            if (User.IsInRole(AppRoles.Admin))
            {
                appointments = await _repository.GetAllAsync();
            }
            else if (User.IsInRole(AppRoles.Doctor))
            {
                var doctorId = await ResolveDoctorIdAsync();
                if (doctorId is null)
                    return Ok(Enumerable.Empty<AppointmentDto>());

                appointments = await _repository.GetByDoctorIdAsync(doctorId.Value);
            }
            else
            {
                return Forbid();
            }

            var dtos = appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                DoctorId = a.DoctorId,
                PatientId = a.PatientId,
                DoctorName = a.Doctor?.FullName,
                PatientName = a.Patient?.FullName,
                Date = a.Date,
                Status = a.Status.ToString()
            });
            return Ok(dtos);
        }

        // ---------------- GET MY APPOINTMENTS (Patient) ----------------
        [Authorize(Roles = AppRoles.Patient)]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var patientId = await ResolvePatientIdAsync();
            if (patientId is null)
                return Ok(Enumerable.Empty<AppointmentDto>());

            var appointments = await _repository.GetByPatientIdAsync(patientId.Value);
            var dtos = appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                DoctorId = a.DoctorId,
                PatientId = a.PatientId,
                DoctorName = a.Doctor?.FullName,
                PatientName = a.Patient?.FullName,
                Date = a.Date,
                Status = a.Status.ToString()
            });
            return Ok(dtos);
        }

        // ---------------- CREATE REQUEST (Patient) ----------------
        [Authorize(Roles = AppRoles.Patient)]
        [HttpPost]
        public async Task<IActionResult> Request([FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var patientId = await ResolvePatientIdAsync();
            if (patientId is null)
                return BadRequest("Patient profile not found.");

            if (!await _db.Doctors.AnyAsync(d => d.Id == dto.DoctorId))
                return BadRequest("Doctor not found.");

            var appointment = new Appointment
            {
                DoctorId = dto.DoctorId,
                PatientId = patientId.Value,
                Date = dto.Date,
                Status = AppointmentStatus.Requested
            };

            var created = await _repository.CreateAsync(appointment);

            var resultDto = new AppointmentDto
            {
                Id = created.Id,
                DoctorId = created.DoctorId,
                PatientId = created.PatientId,
                DoctorName = created.Doctor?.FullName,
                PatientName = created.Patient?.FullName,
                Date = created.Date,
                Status = created.Status.ToString()
            };

            var doctorEmail = await _db.Doctors
                .Where(d => d.Id == created.DoctorId)
                .Select(d => d.Email)
                .FirstOrDefaultAsync();
            var doctorUserId = await ResolveUserIdByEmailAsync(doctorEmail);
            await CreateNotificationAsync(
                doctorUserId,
                "Nouveau RDV",
                $"Un patient a demande un rendez-vous le {created.Date:yyyy-MM-dd HH:mm}.",
                "/Doctor/Appointments");

            return CreatedAtAction(nameof(GetMyAppointments), new { id = resultDto.Id }, resultDto);
        }

        // ---------------- ACCEPT APPOINTMENT (Doctor) ----------------
        [Authorize(Roles = AppRoles.Doctor)]
        [HttpPut("accept/{id}")]
        public async Task<IActionResult> Accept(int id)
        {
            var doctorId = await ResolveDoctorIdAsync();
            if (doctorId is null)
                return BadRequest("Doctor profile not found.");

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.DoctorId != doctorId.Value) return Forbid();

            var updated = await _repository.UpdateStatusAsync(id, AppointmentStatus.Accepted);
            if (updated == null) return NotFound();

            var patientEmail = await _db.Patients
                .Where(p => p.Id == updated.PatientId)
                .Select(p => p.Email)
                .FirstOrDefaultAsync();
            var patientUserId = await ResolveUserIdByEmailAsync(patientEmail);
            await CreateNotificationAsync(
                patientUserId,
                "RDV accepte",
                $"Votre rendez-vous du {updated.Date:yyyy-MM-dd HH:mm} a ete accepte.",
                "/Client/Appointments");

            return Ok(new AppointmentDto
            {
                Id = updated.Id,
                DoctorId = updated.DoctorId,
                PatientId = updated.PatientId,
                DoctorName = updated.Doctor?.FullName,
                PatientName = updated.Patient?.FullName,
                Date = updated.Date,
                Status = updated.Status.ToString()
            });
        }

        // ---------------- REFUSE APPOINTMENT (Doctor) ----------------
        [Authorize(Roles = AppRoles.Doctor)]
        [HttpPut("refuse/{id}")]
        public async Task<IActionResult> Refuse(int id)
        {
            var doctorId = await ResolveDoctorIdAsync();
            if (doctorId is null)
                return BadRequest("Doctor profile not found.");

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.DoctorId != doctorId.Value) return Forbid();

            var updated = await _repository.UpdateStatusAsync(id, AppointmentStatus.Refused);
            if (updated == null) return NotFound();

            var patientEmail = await _db.Patients
                .Where(p => p.Id == updated.PatientId)
                .Select(p => p.Email)
                .FirstOrDefaultAsync();
            var patientUserId = await ResolveUserIdByEmailAsync(patientEmail);
            await CreateNotificationAsync(
                patientUserId,
                "RDV refuse",
                $"Votre rendez-vous du {updated.Date:yyyy-MM-dd HH:mm} a ete refuse.",
                "/Client/Appointments");

            return Ok(new AppointmentDto
            {
                Id = updated.Id,
                DoctorId = updated.DoctorId,
                PatientId = updated.PatientId,
                DoctorName = updated.Doctor?.FullName,
                PatientName = updated.Patient?.FullName,
                Date = updated.Date,
                Status = updated.Status.ToString()
            });
        }

        // ---------------- UPDATE APPOINTMENT (Patient) ----------------
        [Authorize(Roles = AppRoles.Patient)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentUpdateRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var patientId = await ResolvePatientIdAsync();
            if (patientId is null)
                return BadRequest("Patient profile not found.");

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.PatientId != patientId.Value) return Forbid();
            if (existing.Status != AppointmentStatus.Requested)
                return BadRequest("Appointment cannot be modified once it is accepted or refused.");

            if (!await _db.Doctors.AnyAsync(d => d.Id == dto.DoctorId))
                return BadRequest("Doctor not found.");

            existing.DoctorId = dto.DoctorId;
            existing.Date = dto.Date;
            await _db.SaveChangesAsync();

            var updated = await _repository.GetByIdAsync(id) ?? existing;
            return Ok(new AppointmentDto
            {
                Id = updated.Id,
                DoctorId = updated.DoctorId,
                PatientId = updated.PatientId,
                DoctorName = updated.Doctor?.FullName,
                PatientName = updated.Patient?.FullName,
                Date = updated.Date,
                Status = updated.Status.ToString()
            });
        }

        // ---------------- DELETE APPOINTMENT (Patient) ----------------
        [Authorize(Roles = AppRoles.Patient)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var patientId = await ResolvePatientIdAsync();
            if (patientId is null)
                return BadRequest("Patient profile not found.");

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.PatientId != patientId.Value) return Forbid();
            if (existing.Status != AppointmentStatus.Requested)
                return BadRequest("Appointment cannot be deleted once it is accepted or refused.");

            _db.Appointments.Remove(existing);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private async Task<int?> ResolvePatientIdAsync()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var patient = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Email == email);
            return patient?.Id;
        }

        private async Task<int?> ResolveDoctorIdAsync()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var doctor = await _db.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Email == email);
            return doctor?.Id;
        }

        private async Task<string?> ResolveUserIdByEmailAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _db.Users
                .AsNoTracking()
                .Where(u => u.Email == email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
        }

        private async Task CreateNotificationAsync(string? userId, string title, string message, string? linkUrl)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                LinkUrl = linkUrl,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });

            await _db.SaveChangesAsync();
        }
    }
}
