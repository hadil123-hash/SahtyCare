using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sahty.Shared.Auth;
using Sahty.API.Data;
using Sahty.Metiers;
using Sahty.Shared.Data;

namespace Sahty.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = AppRoles.Admin)]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    // -------------------- LIST USERS --------------------
    [HttpGet("users")]
    public async Task<ActionResult> ListUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var normalizedRoles = AppRoles.NormalizeAll(roles);
            result.Add(new
            {
                user.Id,
                user.Email,
                user.UserName,
                user.FullName,
                Roles = normalizedRoles
            });
        }

        return Ok(result);
    }

    // -------------------- CREATE USER --------------------
    public sealed record CreateUserRequest(string Email, string Password, string Role, string? FullName);

    [HttpPost("users")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var normalizedRole = AppRoles.Normalize(req.Role);
        if (!AppRoles.All.Contains(normalizedRole))
            return BadRequest("Unknown role.");
        if (normalizedRole != AppRoles.Admin)
            return BadRequest("Admin can only create admin users. Use role assignment for other accounts.");

        var existing = await _userManager.FindByEmailAsync(req.Email);
        if (existing is not null)
            return Conflict("Email already used.");

        var user = new ApplicationUser
        {
            UserName = req.Email,
            Email = req.Email,
            EmailConfirmed = true,
            FullName = req.FullName
        };

        var create = await _userManager.CreateAsync(user, req.Password);
        if (!create.Succeeded)
            return BadRequest(create.Errors.Select(e => e.Description));

        var addRole = await _userManager.AddToRoleAsync(user, normalizedRole);
        if (!addRole.Succeeded)
            return BadRequest(addRole.Errors.Select(e => e.Description));

        await EnsureDomainProfileAsync(user, normalizedRole);

        return CreatedAtAction(nameof(ListUsers), new { user.Id }, new { user.Id, user.Email, Role = normalizedRole });
    }

    // -------------------- SET ROLE --------------------
    public sealed record SetRoleRequest(string UserId, string Role);

    [HttpPut("users/role")]
    public async Task<ActionResult> SetRole([FromBody] SetRoleRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var normalizedRole = AppRoles.Normalize(req.Role);
        if (!AppRoles.All.Contains(normalizedRole))
            return BadRequest("Unknown role.");

        var user = await _userManager.FindByIdAsync(req.UserId);
        if (user is null) return NotFound("User not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        var normalizedCurrentRoles = AppRoles.NormalizeAll(currentRoles);

        foreach (var role in normalizedCurrentRoles)
        {
            if (string.Equals(role, normalizedRole, StringComparison.OrdinalIgnoreCase))
                continue;

            if (role == AppRoles.Doctor)
            {
                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Email == user.Email);
                if (doctor != null)
                {
                    var hasAppointments = await _db.Appointments.AnyAsync(a => a.DoctorId == doctor.Id);
                    var hasPrescriptions = await _db.Prescriptions.AnyAsync(p => p.DoctorId == doctor.Id);
                    if (hasAppointments || hasPrescriptions)
                        return Conflict("Cannot change role: doctor has appointments or prescriptions.");

                    _db.Doctors.Remove(doctor);
                }
            }
            else if (role == AppRoles.Patient)
            {
                var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Email == user.Email);
                if (patient != null)
                {
                    var hasAppointments = await _db.Appointments.AnyAsync(a => a.PatientId == patient.Id);
                    var hasPrescriptions = await _db.Prescriptions.AnyAsync(p => p.PatientId == patient.Id);
                    if (hasAppointments || hasPrescriptions)
                        return Conflict("Cannot change role: patient has appointments or prescriptions.");

                    _db.Patients.Remove(patient);
                }
            }
            else if (role == AppRoles.Pharmacist)
            {
                var pharmacist = await _db.Pharmacists.FirstOrDefaultAsync(p => p.Email == user.Email);
                if (pharmacist != null)
                {
                    var hasPrescriptions = await _db.Prescriptions.AnyAsync(p => p.PharmacistId == pharmacist.Id);
                    if (hasPrescriptions)
                        return Conflict("Cannot change role: pharmacist has prescriptions.");

                    _db.Pharmacists.Remove(pharmacist);
                }
            }
        }

        if (currentRoles.Any())
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var addRole = await _userManager.AddToRoleAsync(user, normalizedRole);
        if (!addRole.Succeeded)
            return BadRequest(addRole.Errors.Select(e => e.Description));

        await _db.SaveChangesAsync();
        await EnsureDomainProfileAsync(user, normalizedRole);

        return Ok(new { user.Id, user.Email, Role = normalizedRole });
    }

    // -------------------- DELETE USER --------------------
    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest("Invalid user id.");

        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound("User not found.");

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Email == user.Email);
            if (doctor != null)
            {
                var hasAppointments = await _db.Appointments.AnyAsync(a => a.DoctorId == doctor.Id);
                var hasPrescriptions = await _db.Prescriptions.AnyAsync(p => p.DoctorId == doctor.Id);
                if (hasAppointments || hasPrescriptions)
                    return Conflict("Doctor has appointments or prescriptions.");

                _db.Doctors.Remove(doctor);
            }

            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Email == user.Email);
            if (patient != null)
            {
                var hasAppointments = await _db.Appointments.AnyAsync(a => a.PatientId == patient.Id);
                var hasPrescriptions = await _db.Prescriptions.AnyAsync(p => p.PatientId == patient.Id);
                if (hasAppointments || hasPrescriptions)
                    return Conflict("Patient has appointments or prescriptions.");

                _db.Patients.Remove(patient);
            }

            var pharmacist = await _db.Pharmacists.FirstOrDefaultAsync(p => p.Email == user.Email);
            if (pharmacist != null)
            {
                var hasPrescriptions = await _db.Prescriptions.AnyAsync(p => p.PharmacistId == pharmacist.Id);
                if (hasPrescriptions)
                    return Conflict("Pharmacist has prescriptions.");

                _db.Pharmacists.Remove(pharmacist);
            }

            await _db.SaveChangesAsync();
        }

        var deleted = await _userManager.DeleteAsync(user);
        if (!deleted.Succeeded)
            return BadRequest(deleted.Errors.Select(e => e.Description));

        return NoContent();
    }

    private async Task EnsureDomainProfileAsync(ApplicationUser user, string role)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
            return;

        if (role == AppRoles.Doctor)
        {
            var exists = await _db.Doctors.AnyAsync(d => d.Email == user.Email);
            if (!exists)
            {
                _db.Doctors.Add(new Doctor
                {
                    FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName,
                    Email = user.Email,
                    Speciality = "General"
                });
            }
        }
        else if (role == AppRoles.Pharmacist)
        {
            var exists = await _db.Pharmacists.AnyAsync(p => p.Email == user.Email);
            if (!exists)
            {
                _db.Pharmacists.Add(new Pharmacist
                {
                    FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName,
                    Email = user.Email,
                    PharmacyName = "Pharmacy"
                });
            }
        }
        else if (role == AppRoles.Patient)
        {
            var exists = await _db.Patients.AnyAsync(p => p.Email == user.Email);
            if (!exists)
            {
                _db.Patients.Add(new Patient
                {
                    FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName,
                    Email = user.Email,
                    DateOfBirth = DateTime.UtcNow.Date,
                    PhoneNumber = "N/A"
                });
            }
        }

        await _db.SaveChangesAsync();
    }
}
