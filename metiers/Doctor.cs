using System.Collections.Generic;

namespace Sahty.Metiers
{
    public class Doctor
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Speciality { get; set; }
        public string Email { get; set; }

        // Navigation properties
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Prescription> Prescriptions { get; set; }
    }
}
