namespace TherapyCenter.DTO_s.Appointment
{
    public class BookAppointmentRequest
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int TherapyId { get; set; }
        public int SlotId { get; set; }
        public int? ReceptionistId { get; set; }     // null for online bookings
        public string? Notes { get; set; }
    }
}
