using TherapyCenter.Data;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TherapyCenter.Repositories.Implementations
{
    public class SlotRepository : ISlotRepository
    {
        private readonly AppDbContext _context;
        public SlotRepository(AppDbContext context) => _context = context;

        public async Task<Slot?> GetByIdAsync(int slotId)
            => await _context.Slots.FindAsync(slotId);

        public async Task<IEnumerable<Slot>> GetAvailableSlotsByDoctorAsync(int doctorId, DateOnly date)
            => await _context.Slots
                             .Where(s => s.DoctorId == doctorId && s.Date == date && !s.IsBooked)
                             .OrderBy(s => s.StartTime)
                             .ToListAsync();

        public async Task<IEnumerable<Slot>> GetSlotsByDoctorAsync(int doctorId)
            => await _context.Slots
                             .Where(s => s.DoctorId == doctorId)
                             .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                             .ToListAsync();

        public async Task<Slot> CreateAsync(Slot slot)
        {
            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();
            return slot;
        }

        public async Task BulkCreateAsync(IEnumerable<Slot> slots)
        {
            // AddRange is efficient — single INSERT batch, one SaveChangesAsync call
            await _context.Slots.AddRangeAsync(slots);
            await _context.SaveChangesAsync();
        }

        public async Task<Slot> UpdateAsync(Slot slot)
        {
            _context.Slots.Update(slot);
            await _context.SaveChangesAsync();
            return slot;
        }
    }
}
