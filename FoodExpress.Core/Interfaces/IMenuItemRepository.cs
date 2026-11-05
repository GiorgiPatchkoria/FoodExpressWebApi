using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Core.Interfaces
{
    public interface IMenuItemRepository
    {
        Task<MenuItem?> FindAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<MenuItem>> ListAsync(int? restaurantId, MenuQueryDTO query, CancellationToken ct);
        Task<bool> NameExistsAsync(int restaurantId, string name, int? excludingId, CancellationToken ct);
        void Remove(MenuItem entity);
        Task AddAsync(MenuItem item, CancellationToken ct = default);
    }
}