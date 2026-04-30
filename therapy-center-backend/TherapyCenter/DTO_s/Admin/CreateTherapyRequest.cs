namespace TherapyCenter.DTO_s.Admin
{
    public class CreateTherapyRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Cost { get; set; }
    }
}
