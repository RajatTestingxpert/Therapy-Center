namespace TherapyCenter.DTO_s.Admin
{
    public class CreateDoctorProfileRequest
    {
        public int UserId { get; set; }
        public string? Specialization { get; set; }
        public string? Bio { get; set; }
        public string? AvailableDays { get; set; }   // "Mon,Wed,Fri"
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
    }
}
