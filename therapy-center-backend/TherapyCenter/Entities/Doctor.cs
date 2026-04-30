namespace TherapyCenter.Entities
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }              // FK → Users table
        public User User { get; set; } = null!;      // null! tells compiler EF Core will fill this
        public string? Specialization { get; set; }
        public string? Bio { get; set; }
        public string? AvailableDays { get; set; }   // stored as "Mon,Wed,Fri" — parsed in service
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Slot> Slots { get; set; } = new List<Slot>();
    }
}
