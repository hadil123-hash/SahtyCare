using System.Collections.Generic;

namespace Sahty.Metiers
{
    public class Medication
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }

        // Navigation properties
        public ICollection<Prescription> Prescriptions { get; set; }
    }
}
