using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Core.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> FindAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<Order>> ListByUserAsync(int userId, int skip, int take, CancellationToken ct);
        Task<IReadOnlyList<OrderStatusSummaryDTO>> GetStatusSummaryAsync(CancellationToken ct);
        Task<IReadOnlyList<Order>> ListAllAsync(int skip, int take, CancellationToken ct);
        Task AddAsync(Order order, CancellationToken ct = default);
    }
}