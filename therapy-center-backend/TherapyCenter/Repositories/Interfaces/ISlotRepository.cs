using TherapyCenter.Entities;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface ISlotRepository
    {
        Task<Slot?> GetByIdAsync(int slotId);
        Task<IEnumerable<Slot>> GetAvailableSlotsByDoctorAsync(int doctorId, DateOnly date);
        Task<IEnumerable<Slot>> GetSlotsByDoctorAsync(int doctorId);
        Task<Slot> CreateAsync(Slot slot);
        Task BulkCreateAsync(IEnumerable<Slot> slots);
        Task<Slot> UpdateAsync(Slot slot);
    }
}
