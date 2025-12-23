using System.ComponentModel.DataAnnotations;

namespace Sahty.API.DTOs
{
    // DTO pour retourner un pharmacien
    public class PharmacistDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PharmacyName { get; set; }
    }

    // DTO pour créer un pharmacien
    public class PharmacistCreateDto
    {
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PharmacyName { get; set; }

        public string? Password { get; set; }
    }

    // DTO pour mettre à jour un pharmacien
    public class PharmacistUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PharmacyName { get; set; }
    }
}
