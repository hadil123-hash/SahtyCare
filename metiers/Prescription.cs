using System;

namespace Sahty.Metiers
{
    public enum PrescriptionStatus
    {
        Pending,
        Accepted,
        Refused
    }

    public class Prescription
    {
        public int Id { get; set; }

        // Foreign Keys
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int PharmacistId { get; set; }
        public int MedicationId { get; set; }

        // Navigation Properties
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public Pharmacist Pharmacist { get; set; }
        public Medication Medication { get; set; }

        // Autres champs
        public DateTime DateIssued { get; set; }
        public string Dosage { get; set; }
        public string Notes { get; set; }
        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;
    }
}
