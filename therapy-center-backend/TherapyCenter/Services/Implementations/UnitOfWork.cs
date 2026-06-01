using TherapyCenter.Data;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
            => await _context.Database.BeginTransactionAsync();

        public async Task CommitAsync()
            => await _context.Database.CommitTransactionAsync();

        public async Task RollbackAsync()
            => await _context.Database.RollbackTransactionAsync();
    }
}