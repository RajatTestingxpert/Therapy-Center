using TherapyCenter.Data;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TherapyCenter.Repositories.Implementations
{
    public class TherapyRepository : ITherapyRepository
    {
        private readonly AppDbContext _context;
        public TherapyRepository(AppDbContext context) => _context = context;

        public async Task<Therapy?> GetByIdAsync(int therapyId)
            => await _context.Therapies.FindAsync(therapyId);

        public async Task<IEnumerable<Therapy>> GetAllAsync()
            => await _context.Therapies.ToListAsync();

        public async Task<Therapy> CreateAsync(Therapy therapy)
        {
            _context.Therapies.Add(therapy);
            await _context.SaveChangesAsync();
            return therapy;
        }

        public async Task<Therapy> UpdateAsync(Therapy therapy)
        {
            _context.Therapies.Update(therapy);
            await _context.SaveChangesAsync();
            return therapy;
        }

        public async Task DeleteAsync(int therapyId)
        {
            var therapy = await _context.Therapies.FindAsync(therapyId);
            if (therapy != null)
            {
                _context.Therapies.Remove(therapy);
                await _context.SaveChangesAsync();
            }
        }
    }
}
