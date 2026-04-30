namespace TherapyCenter.Entities
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public int TherapyId { get; set; }
        public Therapy Therapy { get; set; } = null!;
        public int? ReceptionistId { get; set; }     // null for online bookings
        public User? Receptionist { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled | Completed | Cancelled
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DoctorFinding? Finding { get; set; }
        public Payment? Payment { get; set; }
    }
}
