namespace TherapyCenter.Entities
{
    public class DoctorFinding
    {
        public int FindingId { get; set; }
        public int AppointmentId { get; set; }       // 1-to-1 with Appointment
        public Appointment Appointment { get; set; } = null!;
        public string? Observations { get; set; }
        public string? Recommendations { get; set; }
        public DateOnly? NextSessionDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
