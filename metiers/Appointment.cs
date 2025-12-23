using System;

namespace Sahty.Metiers
{
    public enum AppointmentStatus
    {
        Requested,
        Accepted,
        Refused
    }

    public class Appointment
    {
        public int Id { get; set; }

        // Foreign Keys
        public int DoctorId { get; set; }
        public int PatientId { get; set; }

        // Navigation Properties
        public Doctor Doctor { get; set; }
        public Patient Patient { get; set; }

        public DateTime Date { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Requested;
    }
}
