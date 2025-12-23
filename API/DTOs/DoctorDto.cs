using System.ComponentModel.DataAnnotations;

namespace Sahty.API.DTOs
{
    // DTO pour lister ou retourner un doctor
    public class DoctorDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Speciality { get; set; }
        public string Email { get; set; }
    }

    // DTO pour créer un doctor
    public class DoctorCreateDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Speciality { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string? Password { get; set; }
    }

    // DTO pour mettre à jour un doctor
    public class DoctorUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Speciality { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
