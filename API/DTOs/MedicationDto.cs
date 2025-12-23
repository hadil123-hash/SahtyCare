using System.ComponentModel.DataAnnotations;

namespace Sahty.API.DTOs
{
    // DTO pour retourner un médicament
    public class MedicationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
    }

    // DTO pour créer un médicament
    public class MedicationCreateDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be non-negative")]
        public int Stock { get; set; }

        public string Description { get; set; }
    }

    // DTO pour mettre à jour un médicament
    public class MedicationUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be non-negative")]
        public int Stock { get; set; }

        public string Description { get; set; }
    }
}
