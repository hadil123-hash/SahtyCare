using System.Collections.Generic;

namespace Sahty.Metiers
{
    public class Pharmacist
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PharmacyName { get; set; }

        // Navigation properties
        public ICollection<Prescription> Prescriptions { get; set; }
    }
}
