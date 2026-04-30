namespace TherapyCenter.Entities
{
    public class Therapy
    {
        public int TherapyId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int DurationMinutes { get; set; }   // 30, 45, 60
        public decimal Cost { get; set; }

        // Navigation
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
