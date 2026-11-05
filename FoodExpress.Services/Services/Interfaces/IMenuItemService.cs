using FoodExpress.Core.Models.DTOs;

namespace FoodExpress.Services.Services.Interfaces
{
    public interface IMenuItemService
    {
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<MenuItemDTO> GetAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<MenuItemDTO>> ListAsync(int? restaurantId, MenuQueryDTO query, CancellationToken ct = default);
        Task<MenuItemDTO> CreateAsync(CreateMenuItemDTO request, CancellationToken ct = default);
        Task<MenuItemDTO> UpdateAsync(int id, UpdateMenuItemDTO request, CancellationToken ct = default);
    }
}