using System;
using System.ComponentModel.DataAnnotations;

namespace Sahty.API.DTOs
{
    // DTO pour retourner une ordonnance
    public class PrescriptionDto
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
        public string Dosage { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; } = "";
    }

    // DTO pour créer une ordonnance
    public class PrescriptionCreateDto
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int PharmacistId { get; set; }

        [Required]
        public int MedicationId { get; set; }

        [Required]
        public DateTime DateIssued { get; set; }

        [Required]
        public string Dosage { get; set; }

        public string Notes { get; set; }
    }

    // DTO pour mettre à jour une ordonnance
    public class PrescriptionUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int PharmacistId { get; set; }

        [Required]
        public int MedicationId { get; set; }

        [Required]
        public DateTime DateIssued { get; set; }

        [Required]
        public string Dosage { get; set; }

        public string Notes { get; set; }
        public string? Status { get; set; }
    }
}
