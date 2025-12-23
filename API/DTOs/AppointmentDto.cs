using System;
using System.ComponentModel.DataAnnotations;

namespace Sahty.API.DTOs
{
    // DTO pour retourner un rendez-vous
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }

    // DTO pour créer un rendez-vous
    public class AppointmentCreateDto
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Status { get; set; }
    }

    // DTO pour mettre à jour un rendez-vous
    public class AppointmentUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Status { get; set; }
    }

    public class AppointmentUpdateRequestDto
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
