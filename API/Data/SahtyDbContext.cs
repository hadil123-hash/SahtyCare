using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sahty.Metiers;
using Sahty.Shared.Data;

namespace Sahty.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Pharmacist> Pharmacists { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Appointment -> Patient/Doctor
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription -> Patient
            builder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany(pat => pat.Prescriptions)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription -> Doctor
            builder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany(d => d.Prescriptions)
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription -> Pharmacist
            builder.Entity<Prescription>()
                .HasOne(p => p.Pharmacist)
                .WithMany(ph => ph.Prescriptions)
                .HasForeignKey(p => p.PharmacistId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription -> Medication
            builder.Entity<Prescription>()
                .HasOne(p => p.Medication)
                .WithMany(m => m.Prescriptions)
                .HasForeignKey(p => p.MedicationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

