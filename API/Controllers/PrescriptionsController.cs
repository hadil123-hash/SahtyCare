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
using System.Threading.Tasks;
using System.Security.Claims;

namespace Sahty.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionRepository _repository;
        private readonly ApplicationDbContext _db;

        public PrescriptionsController(IPrescriptionRepository repository, ApplicationDbContext db)
        {
            _repository = repository;
            _db = db;
        }

        // GET: api/prescriptions
        // Accessible aux Doctor et Pharmacist
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor},{AppRoles.Pharmacist}")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<Prescription> prescriptions;

            if (User.IsInRole(AppRoles.Admin))
            {
                prescriptions = await _repository.GetAllAsync();
            }
            else if (User.IsInRole(AppRoles.Doctor))
            {
                var doctorId = await ResolveDoctorIdAsync();
                if (doctorId is null)
                    return Ok(Enumerable.Empty<PrescriptionDto>());

                prescriptions = await _repository.GetByDoctorIdAsync(doctorId.Value);
            }
            else if (User.IsInRole(AppRoles.Pharmacist))
            {
                var pharmacistId = await ResolvePharmacistIdAsync();
                if (pharmacistId is null)
                    return Ok(Enumerable.Empty<PrescriptionDto>());

                prescriptions = await _repository.GetByPharmacistIdAsync(pharmacistId.Value);
            }
            else
            {
                return Forbid();
            }

            var dtos = prescriptions.Select(p => new PrescriptionDto
            {
                Id = p.Id,
                DoctorId = p.DoctorId,
                PatientId = p.PatientId,
                PharmacistId = p.PharmacistId,
                MedicationId = p.MedicationId,
                DoctorName = p.Doctor?.FullName,
                PatientName = p.Patient?.FullName,
                PharmacistName = p.Pharmacist?.FullName,
                MedicationName = p.Medication?.Name,
                DateIssued = p.DateIssued,
                Dosage = p.Dosage,
                Notes = p.Notes,
                Status = p.Status.ToString()
            });
            return Ok(dtos);
        }

        // GET: api/prescriptions/my
        // Accessible aux Patients uniquement
        [Authorize(Roles = AppRoles.Patient)]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyPrescriptions()
        {
            var patientId = await ResolvePatientIdAsync();
            if (patientId is null)
                return Ok(Enumerable.Empty<PrescriptionDto>());

            var prescriptions = await _repository.GetByPatientIdAsync(patientId.Value);
            var dtos = prescriptions.Select(p => new PrescriptionDto
            {
                Id = p.Id,
                DoctorId = p.DoctorId,
                PatientId = p.PatientId,
                PharmacistId = p.PharmacistId,
                MedicationId = p.MedicationId,
                DoctorName = p.Doctor?.FullName,
                PatientName = p.Patient?.FullName,
                PharmacistName = p.Pharmacist?.FullName,
                MedicationName = p.Medication?.Name,
                DateIssued = p.DateIssued,
                Dosage = p.Dosage,
                Notes = p.Notes,
                Status = p.Status.ToString()
            });
            return Ok(dtos);
        }

        // POST: api/prescriptions
        // Accessible aux Doctors uniquement
        [Authorize(Roles = AppRoles.Doctor)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PrescriptionCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var doctorId = await ResolveDoctorIdAsync();
            if (doctorId is null)
                return BadRequest("Doctor profile not found.");

            if (!await _db.Patients.AnyAsync(p => p.Id == dto.PatientId))
                return BadRequest("Patient not found.");
            if (!await _db.Pharmacists.AnyAsync(p => p.Id == dto.PharmacistId))
                return BadRequest("Pharmacist not found.");
            if (!await _db.Medications.AnyAsync(m => m.Id == dto.MedicationId))
                return BadRequest("Medication not found.");

            var prescription = new Prescription
            {
                DoctorId = doctorId.Value,
                PatientId = dto.PatientId,
                PharmacistId = dto.PharmacistId,
                MedicationId = dto.MedicationId,
                DateIssued = dto.DateIssued,
                Dosage = dto.Dosage,
                Notes = dto.Notes,
                Status = PrescriptionStatus.Pending
            };

            var created = await _repository.CreateAsync(prescription);

            var resultDto = new PrescriptionDto
            {
                Id = created.Id,
                DoctorId = created.DoctorId,
                PatientId = created.PatientId,
                PharmacistId = created.PharmacistId,
                MedicationId = created.MedicationId,
                DoctorName = created.Doctor?.FullName,
                PatientName = created.Patient?.FullName,
                PharmacistName = created.Pharmacist?.FullName,
                MedicationName = created.Medication?.Name,
                DateIssued = created.DateIssued,
                Dosage = created.Dosage,
                Notes = created.Notes,
                Status = created.Status.ToString()
            };

            var pharmacistEmail = await _db.Pharmacists
                .Where(p => p.Id == created.PharmacistId)
                .Select(p => p.Email)
                .FirstOrDefaultAsync();
            var pharmacistUserId = await ResolveUserIdByEmailAsync(pharmacistEmail);
            await CreateNotificationAsync(
                pharmacistUserId,
                "Nouvelle ordonnance",
                "Une ordonnance est disponible a traiter.",
                "/Pharmacy/Prescriptions");

            return CreatedAtAction(nameof(GetAll), new { id = resultDto.Id }, resultDto);
        }

        // PUT: api/prescriptions/{id}
        // Accessible aux Pharmacists uniquement
        [Authorize(Roles = AppRoles.Pharmacist)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PrescriptionUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.Id) return BadRequest();

            var pharmacistId = await ResolvePharmacistIdAsync();
            if (pharmacistId is null)
                return BadRequest("Pharmacist profile not found.");

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.PharmacistId != pharmacistId.Value) return Forbid();

            if (!await _db.Patients.AnyAsync(p => p.Id == dto.PatientId))
                return BadRequest("Patient not found.");
            if (!await _db.Doctors.AnyAsync(d => d.Id == dto.DoctorId))
                return BadRequest("Doctor not found.");
            if (!await _db.Medications.AnyAsync(m => m.Id == dto.MedicationId))
                return BadRequest("Medication not found.");

            var previousStatus = existing.Status;
            var status = existing.Status;
            if (!string.IsNullOrWhiteSpace(dto.Status) &&
                Enum.TryParse<PrescriptionStatus>(dto.Status, true, out var parsedStatus))
            {
                status = parsedStatus;
            }
            else
            {
                status = PrescriptionStatus.Accepted;
            }

            var prescription = new Prescription
            {
                Id = dto.Id,
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
                PharmacistId = pharmacistId.Value,
                MedicationId = dto.MedicationId,
                DateIssued = dto.DateIssued,
                Dosage = dto.Dosage,
                Notes = dto.Notes,
                Status = status
            };

            var updated = await _repository.UpdateAsync(prescription);
            if (!updated) return NotFound();

            var resultDto = new PrescriptionDto
            {
                Id = prescription.Id,
                DoctorId = prescription.DoctorId,
                PatientId = prescription.PatientId,
                PharmacistId = prescription.PharmacistId,
                MedicationId = prescription.MedicationId,
                DoctorName = null,
                PatientName = null,
                PharmacistName = null,
                MedicationName = null,
                DateIssued = prescription.DateIssued,
                Dosage = prescription.Dosage,
                Notes = prescription.Notes,
                Status = prescription.Status.ToString()
            };

            if (previousStatus != status && status == PrescriptionStatus.Accepted)
            {
                await NotifyPrescriptionAcceptedAsync(existing);
            }

            return Ok(resultDto);
        }

        // PUT: api/prescriptions/accept/{id}
        // Accessible aux Pharmacists uniquement
        [Authorize(Roles = AppRoles.Pharmacist)]
        [HttpPut("accept/{id}")]
        public async Task<IActionResult> Accept(int id)
        {
            var pharmacistId = await ResolvePharmacistIdAsync();
            if (pharmacistId is null)
                return BadRequest("Pharmacist profile not found.");

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.PharmacistId != pharmacistId.Value) return Forbid();

            if (existing.Status == PrescriptionStatus.Accepted)
                return Ok(new PrescriptionDto
                {
                    Id = existing.Id,
                    DoctorId = existing.DoctorId,
                    PatientId = existing.PatientId,
                    PharmacistId = existing.PharmacistId,
                    MedicationId = existing.MedicationId,
                    DoctorName = existing.Doctor?.FullName,
                    PatientName = existing.Patient?.FullName,
                    PharmacistName = existing.Pharmacist?.FullName,
                    MedicationName = existing.Medication?.Name,
                    DateIssued = existing.DateIssued,
                    Dosage = existing.Dosage,
                    Notes = existing.Notes,
                    Status = existing.Status.ToString()
                });

            existing.Status = PrescriptionStatus.Accepted;

            var updated = await _repository.UpdateAsync(new Prescription
            {
                Id = existing.Id,
                DoctorId = existing.DoctorId,
                PatientId = existing.PatientId,
                PharmacistId = existing.PharmacistId,
                MedicationId = existing.MedicationId,
                DateIssued = existing.DateIssued,
                Dosage = existing.Dosage,
                Notes = existing.Notes,
                Status = existing.Status
            });

            if (!updated) return NotFound();

            await NotifyPrescriptionAcceptedAsync(existing);

            return Ok(new PrescriptionDto
            {
                Id = existing.Id,
                DoctorId = existing.DoctorId,
                PatientId = existing.PatientId,
                PharmacistId = existing.PharmacistId,
                MedicationId = existing.MedicationId,
                DoctorName = existing.Doctor?.FullName,
                PatientName = existing.Patient?.FullName,
                PharmacistName = existing.Pharmacist?.FullName,
                MedicationName = existing.Medication?.Name,
                DateIssued = existing.DateIssued,
                Dosage = existing.Dosage,
                Notes = existing.Notes,
                Status = existing.Status.ToString()
            });
        }

        // DELETE: api/prescriptions/{id}
        // Accessible aux Doctor et Pharmacist
        [Authorize(Roles = $"{AppRoles.Doctor},{AppRoles.Pharmacist}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            if (User.IsInRole(AppRoles.Doctor))
            {
                var doctorId = await ResolveDoctorIdAsync();
                if (doctorId is null) return BadRequest("Doctor profile not found.");
                if (existing.DoctorId != doctorId.Value) return Forbid();
            }

            if (User.IsInRole(AppRoles.Pharmacist))
            {
                var pharmacistId = await ResolvePharmacistIdAsync();
                if (pharmacistId is null) return BadRequest("Pharmacist profile not found.");
                if (existing.PharmacistId != pharmacistId.Value) return Forbid();
            }

            var deleted = await _repository.DeleteAsync(id);
            if (!deleted) return NotFound();

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

        private async Task<int?> ResolvePharmacistIdAsync()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var pharmacist = await _db.Pharmacists.AsNoTracking().FirstOrDefaultAsync(p => p.Email == email);
            return pharmacist?.Id;
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

        private async Task NotifyPrescriptionAcceptedAsync(Prescription prescription)
        {
            var patientEmail = await _db.Patients
                .Where(p => p.Id == prescription.PatientId)
                .Select(p => p.Email)
                .FirstOrDefaultAsync();
            var doctorEmail = await _db.Doctors
                .Where(d => d.Id == prescription.DoctorId)
                .Select(d => d.Email)
                .FirstOrDefaultAsync();

            var patientUserId = await ResolveUserIdByEmailAsync(patientEmail);
            var doctorUserId = await ResolveUserIdByEmailAsync(doctorEmail);

            await CreateNotificationAsync(
                patientUserId,
                "Ordonnance acceptee",
                "Votre ordonnance est acceptee par la pharmacie.",
                "/Client/Prescriptions");

            await CreateNotificationAsync(
                doctorUserId,
                "Ordonnance acceptee",
                "La pharmacie a accepte l'ordonnance.",
                "/Doctor/Prescriptions");
        }
    }
}
