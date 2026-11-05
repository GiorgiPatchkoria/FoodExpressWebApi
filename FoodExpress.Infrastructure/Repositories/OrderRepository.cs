using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;
using FoodExpress.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FoodExpress.Core.Interfaces;

namespace FoodExpress.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FoodDbContext _context;

        public OrderRepository(FoodDbContext db)
        {
            _context = db;
        }

        public Task<Order?> FindAsync(int id, CancellationToken ct)
        {
            return _context.Orders.Include(x => x.Address).Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IReadOnlyList<Order>> ListByUserAsync(int userId, int skip, int take, CancellationToken ct)
        {
            return await _context.Orders.Include(x => x.Items).Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Skip(skip).Take(take).ToListAsync(ct);
        }

        public async Task<IReadOnlyList<OrderStatusSummaryDTO>> GetStatusSummaryAsync(CancellationToken ct)
        {
            return await _context.Orders.GroupBy(x => x.Status)
            .Select(group => new OrderStatusSummaryDTO(group.Key, group.Count(), group.Sum(x => x.TotalPrice))).ToListAsync(ct);
        }

        public async Task AddAsync(Order order, CancellationToken ct = default)
        {
            await _context.Orders.AddAsync(order, ct);
        }

        public async Task<IReadOnlyList<Order>> ListAllAsync(int skip, int take, CancellationToken ct)
        {
            return await _context.Orders.Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        }
    }
}