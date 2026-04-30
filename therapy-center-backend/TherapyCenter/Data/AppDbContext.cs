using System.Reflection.Emit;
using TherapyCenter.Entities;
using Microsoft.EntityFrameworkCore;

namespace TherapyCenter.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Therapy> Therapies => Set<Therapy>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<DoctorFinding> DoctorFindings => Set<DoctorFinding>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Slot> Slots => Set<Slot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ── User ──────────────────────────────────────────────────────────
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.UserId);
                e.Property(u => u.Email).HasMaxLength(100).IsRequired();
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Role).HasMaxLength(20).IsRequired();
                e.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                e.Property(u => u.LastName).HasMaxLength(50).IsRequired();
                e.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
                e.Property(u => u.PhoneNumber).HasMaxLength(20);
            });

            // ── Patient ───────────────────────────────────────────────────────
            modelBuilder.Entity<Patient>(e =>
            {
                e.HasKey(p => p.PatientId);
                e.Property(p => p.FirstName).HasMaxLength(50).IsRequired();
                e.Property(p => p.LastName).HasMaxLength(50).IsRequired();

                e.HasOne(p => p.User)
                 .WithOne()
                 .HasForeignKey<Patient>(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(p => p.Guardian)
                 .WithMany(u => u.GuardedPatients)
                 .HasForeignKey(p => p.GuardianId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ── Doctor (1-to-1 with User) ─────────────────────────────────────
            modelBuilder.Entity<Doctor>(e =>
            {
                e.HasKey(d => d.DoctorId);

                e.HasOne(d => d.User)
                 .WithOne(u => u.DoctorProfile)
                 .HasForeignKey<Doctor>(d => d.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Appointment ───────────────────────────────────────────────────
            modelBuilder.Entity<Appointment>(e =>
            {
                e.HasKey(a => a.AppointmentId);
                e.Property(a => a.Status).HasMaxLength(20).HasDefaultValue("Scheduled");

                e.HasOne(a => a.Patient)
                 .WithMany(p => p.Appointments)
                 .HasForeignKey(a => a.PatientId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Doctor)
                 .WithMany(d => d.Appointments)
                 .HasForeignKey(a => a.DoctorId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Therapy)
                 .WithMany(t => t.Appointments)
                 .HasForeignKey(a => a.TherapyId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Receptionist)
                 .WithMany(u => u.BookedAppointments)
                 .HasForeignKey(a => a.ReceptionistId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ── DoctorFinding (1-to-1 with Appointment) ───────────────────────
            modelBuilder.Entity<DoctorFinding>(e =>
            {
                e.HasKey(f => f.FindingId);

                e.HasOne(f => f.Appointment)
                 .WithOne(a => a.Finding)
                 .HasForeignKey<DoctorFinding>(f => f.AppointmentId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Payment (1-to-1 with Appointment) ────────────────────────────
            modelBuilder.Entity<Payment>(e =>
            {
                e.HasKey(p => p.PaymentId);
                e.Property(p => p.Amount).HasPrecision(10, 2);
                e.Property(p => p.Status).HasMaxLength(20).HasDefaultValue("Pending");

                e.HasOne(p => p.Appointment)
                 .WithOne(a => a.Payment)
                 .HasForeignKey<Payment>(p => p.AppointmentId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Slot ──────────────────────────────────────────────────────────
            modelBuilder.Entity<Slot>(e =>
            {
                e.HasKey(s => s.SlotId);

                e.HasOne(s => s.Doctor)
                 .WithMany(d => d.Slots)
                 .HasForeignKey(s => s.DoctorId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Therapy ───────────────────────────────────────────────────────
            modelBuilder.Entity<Therapy>(e =>
            {
                e.HasKey(t => t.TherapyId);
                e.Property(t => t.Name).HasMaxLength(100).IsRequired();
                e.Property(t => t.Cost).HasPrecision(10, 2);
            });
        }
    }
}
