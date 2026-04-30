using System.Numerics;

namespace TherapyCenter.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin | Receptionist | Doctor | Patient | Guardian
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties — EF Core uses these to build JOINs
        public Doctor? DoctorProfile { get; set; }
        public ICollection<Patient> GuardedPatients { get; set; } = new List<Patient>();
        public ICollection<Appointment> BookedAppointments { get; set; } = new List<Appointment>();
    }
}
