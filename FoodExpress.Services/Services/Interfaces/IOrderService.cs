using FoodExpress.Core.Models.DTOs;

namespace FoodExpress.Services.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDetailsDTO> CreateAsync(int userId, CreateOrderDTO request, CancellationToken ct = default);
        Task<IReadOnlyList<OrderDTO>> ListAsync(int userId, PageDTO page, CancellationToken ct = default);
        Task<OrderDetailsDTO> CancelAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<OrderStatusSummaryDTO>> SummaryAsync(CancellationToken ct = default);
        Task<IReadOnlyList<OrderDTO>> ListAllAsync(PageDTO page, CancellationToken ct = default);
        Task<OrderDetailsDTO> GetAsync(int id, CancellationToken ct = default);
        Task<OrderDetailsDTO> UpdateStatusAsync(int id, UpdateOrderStatusDTO request, CancellationToken ct = default);
    }
}