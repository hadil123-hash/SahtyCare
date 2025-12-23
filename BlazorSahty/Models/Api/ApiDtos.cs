using System;

namespace Sahty.Web.Models.Api;

public sealed class AuthResponseDto
{
    public string Token { get; set; } = "";
    public DateTimeOffset ExpiresAt { get; set; }
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string[] Roles { get; set; } = Array.Empty<string>();
}

public sealed class LoginDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class RegisterDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string FullName { get; set; } = "";
}

public sealed class AdminUserDto
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}

public sealed class AdminCreateUserRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
    public string? FullName { get; set; }
}

public sealed class AdminSetRoleRequest
{
    public string UserId { get; set; } = "";
    public string Role { get; set; } = "";
}

public sealed class AdminUserResponseDto
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}

public sealed class AppointmentDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = "";
}

public sealed class AppointmentCreateDto
{
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = "";
}

public sealed class AppointmentUpdateRequestDto
{
    public int DoctorId { get; set; }
    public DateTime Date { get; set; }
}

public sealed class DoctorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Speciality { get; set; } = "";
    public string Email { get; set; } = "";
}

public sealed class DoctorCreateDto
{
    public string FullName { get; set; } = "";
    public string Speciality { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Password { get; set; }
}

public sealed class DoctorUpdateDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Speciality { get; set; } = "";
    public string Email { get; set; } = "";
}

public sealed class PatientDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = "";
}

public sealed class PatientCreateDto
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = "";
    public string? Password { get; set; }
}

public sealed class PatientUpdateDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = "";
}

public sealed class PharmacistDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PharmacyName { get; set; } = "";
}

public sealed class PharmacistCreateDto
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PharmacyName { get; set; } = "";
    public string? Password { get; set; }
}

public sealed class PharmacistUpdateDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PharmacyName { get; set; } = "";
}

public sealed class MedicationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Stock { get; set; }
    public string Description { get; set; } = "";
}

public sealed class MedicationCreateDto
{
    public string Name { get; set; } = "";
    public int Stock { get; set; }
    public string Description { get; set; } = "";
}

public sealed class MedicationUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Stock { get; set; }
    public string Description { get; set; } = "";
}

public sealed class PrescriptionDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public int PharmacistId { get; set; }
    public int MedicationId { get; set; }
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
    public string? PharmacistName { get; set; }
    public string? MedicationName { get; set; }
    public DateTime DateIssued { get; set; }
    public string Dosage { get; set; } = "";
    public string? Notes { get; set; }
    public string Status { get; set; } = "";
}

public sealed class PrescriptionCreateDto
{
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public int PharmacistId { get; set; }
    public int MedicationId { get; set; }
    public DateTime DateIssued { get; set; }
    public string Dosage { get; set; } = "";
    public string? Notes { get; set; }
}

public sealed class PrescriptionUpdateDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public int PharmacistId { get; set; }
    public int MedicationId { get; set; }
    public DateTime DateIssued { get; set; }
    public string Dosage { get; set; } = "";
    public string? Notes { get; set; }
    public string? Status { get; set; }
}

public sealed class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string? LinkUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
