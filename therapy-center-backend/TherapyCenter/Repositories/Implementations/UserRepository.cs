using TherapyCenter.Data;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TherapyCenter.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) => _context = context;

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        public async Task<User?> GetByIdAsync(int userId)
            => await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
            => await _context.Users.Where(u => u.Role == role && u.IsActive).ToListAsync();

        public async Task<bool> EmailExistsAsync(string email)
            => await _context.Users.AnyAsync(u => u.Email == email);

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
