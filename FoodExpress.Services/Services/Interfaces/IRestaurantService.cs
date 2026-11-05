using FoodExpress.Core.Models.DTOs;

namespace FoodExpress.Services.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<RestaurantDTO> GetAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<RestaurantDTO>> ListAsync(RestaurantQueryDTO query, CancellationToken ct = default);
        Task<RestaurantDTO> CreateAsync(CreateRestaurantDTO request, CancellationToken ct = default);
        Task<RestaurantDTO> UpdateAsync(int id, UpdateRestaurantDTO request, CancellationToken ct = default);
    }
}