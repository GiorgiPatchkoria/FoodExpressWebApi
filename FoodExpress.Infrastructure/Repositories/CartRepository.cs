using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.Entities;
using FoodExpress.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly FoodDbContext _context;

        public CartRepository(FoodDbContext db)
        {
            _context = db;
        }

        public Task<Cart?> FindByUserAsync(int userId, CancellationToken ct)
        {
            return _context.Carts.Include(x => x.Items).ThenInclude(x => x.MenuItem).ThenInclude(x => x.Restaurant)
            .SingleOrDefaultAsync(x => x.UserId == userId, ct);
        }

        public async Task AddAsync(Cart cart, CancellationToken ct = default)
        {
            await _context.Carts.AddAsync(cart, ct);
        }

        public void RemoveItem(CartItem item)
        {
            _context.CartItems.Remove(item);
        }
    }
}