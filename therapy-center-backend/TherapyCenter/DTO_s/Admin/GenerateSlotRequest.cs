namespace TherapyCenter.DTO_s.Admin
{
    public class GenerateSlotsRequest   // rename from GenrateSlotRequest
    {
        public int DoctorId { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
    }
}