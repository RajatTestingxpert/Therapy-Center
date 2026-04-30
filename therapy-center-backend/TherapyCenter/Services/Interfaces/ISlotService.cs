using TherapyCenter.Entities;

namespace TherapyCenter.Services.Interfaces
{
    public interface ISlotService
    {
        Task<IEnumerable<Slot>> GetAvailableAsync(int doctorId, DateOnly date);
    }

}
