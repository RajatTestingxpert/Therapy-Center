using TherapyCenter.Entities;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface ITherapyRepository
    {
        Task<Therapy?> GetByIdAsync(int therapyId);
        Task<IEnumerable<Therapy>> GetAllAsync();
        Task<Therapy> CreateAsync(Therapy therapy);
        Task<Therapy> UpdateAsync(Therapy therapy);
        Task DeleteAsync(int therapyId);
    }
}
