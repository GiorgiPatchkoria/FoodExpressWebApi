using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Core.Interfaces
{
    public interface IRestaurantRepository
    {
        Task<Restaurant?> FindAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<Restaurant>> ListAsync(RestaurantQueryDTO query, CancellationToken ct);
        void Remove(Restaurant entity);
        Task AddAsync(Restaurant restaurant, CancellationToken ct = default);
    }
}