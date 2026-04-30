namespace TherapyCenter.DTO_s.Patient
{
    public class CreatePatientRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? MedicalHistory { get; set; }
        public int? GuardianId { get; set; }
    }
}
