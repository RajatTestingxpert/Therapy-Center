using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class SlotService : ISlotService
    {
        private readonly ISlotRepository _slotRepo;

        public SlotService(ISlotRepository slotRepo)
        {
            _slotRepo = slotRepo;
        }

        public async Task<IEnumerable<Slot>> GetAvailableAsync(int doctorId, DateOnly date)
            => await _slotRepo.GetAvailableSlotsByDoctorAsync(doctorId, date);
    }
}
