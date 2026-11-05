using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IReadOnlyList<Category>> ListAsync(int skip, int take, CancellationToken ct = default);
        Task<Category?> FindAsync(int id, CancellationToken ct = default);
        Task<bool> NameExistsAsync(string name, CancellationToken ct = default);
        Task<bool> CategoryExistsAsync(int id, CancellationToken ct = default);
        Task AddAsync(Category category, CancellationToken ct = default);
    }
}