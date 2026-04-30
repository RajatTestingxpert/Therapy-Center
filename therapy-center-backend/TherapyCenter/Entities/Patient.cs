namespace TherapyCenter.Entities
{
    public class Patient
    {
        public int PatientId { get; set; }
        public int? UserId { get; set; }            // optional link to a self-registered Patient user
        public User? User { get; set; }

        public int? GuardianId { get; set; }        // nullable — patient can exist without a registered guardian
        public User? Guardian { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? MedicalHistory { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
