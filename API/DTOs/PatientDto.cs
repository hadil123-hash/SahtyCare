using System;
using System.ComponentModel.DataAnnotations;

namespace Sahty.API.DTOs
{
    // DTO pour retourner un patient
    public class PatientDto
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string PhoneNumber { get; set; }
    }

    // DTO pour créer un patient
    public class PatientCreateDto
    {
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        public string? Password { get; set; }
    }

    // DTO pour mettre à jour un patient
    public class PatientUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }
    }
}
