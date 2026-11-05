using FoodExpress.Core.Models.DTOs;

namespace FoodExpress.Services.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDTO> GetAsync(int userId, CancellationToken ct = default);
        Task<CartDTO> AddItemAsync(int userId, AddCartItemDTO request, CancellationToken ct = default);
        Task<CartDTO> UpdateItemAsync(int userId, int menuItemId, UpdateCartItemDTO request, CancellationToken ct = default);
        Task<CartDTO> RemoveItemAsync(int userId, int menuItemId, CancellationToken ct = default);
    }
}