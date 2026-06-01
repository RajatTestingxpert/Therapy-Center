namespace TherapyCenter.DTO_s.Finding
{
    public class UpsertFindingRequest
    {
        public string? Observations { get; set; }
        public string? Recommendations { get; set; }
        public DateOnly? NextSessionDate { get; set; }
    }
}