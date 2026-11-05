using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Core.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> FindByUserAsync(int userId, CancellationToken ct);
        Task AddAsync(Cart cart, CancellationToken ct = default);
        void RemoveItem(CartItem item);
    }
}