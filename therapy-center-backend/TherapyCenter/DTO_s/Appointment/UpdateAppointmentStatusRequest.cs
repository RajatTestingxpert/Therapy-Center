namespace TherapyCenter.DTO_s.Appointment
{
    public class UpdateAppointmentStatusRequest
    {
        public string Status { get; set; } = string.Empty;  // Scheduled | Completed | Cancelled
        public string? Notes { get; set; }
    }
}

