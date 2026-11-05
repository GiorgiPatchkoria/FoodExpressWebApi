using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;
using FoodExpress.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FoodExpress.Core.Interfaces;

namespace FoodExpress.Infrastructure.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly FoodDbContext _context;

        public RestaurantRepository(FoodDbContext db)
        {
            _context = db;
        }

        public Task<Restaurant?> FindAsync(int id, CancellationToken ct)
        {
            return _context.Restaurants.SingleOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IReadOnlyList<Restaurant>> ListAsync(RestaurantQueryDTO query, CancellationToken ct)
        {
            var rows = _context.Restaurants.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                rows = rows.Where(x => x.Name.Contains(query.Search.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(query.City))
            {
                rows = rows.Where(x => x.City == query.City.Trim());
            }

            if (query.IsActive.HasValue)
            {
                rows = rows.Where(x => x.IsActive == query.IsActive.Value);
            }

            return await rows.OrderBy(x => x.Name).ThenBy(x => x.Id)
                .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        }

        public void Remove(Restaurant entity)
        {
            _context.Restaurants.Remove(entity);
        }

        public async Task AddAsync(Restaurant restaurant, CancellationToken ct = default)
        {
            await _context.Restaurants.AddAsync(restaurant, ct);
        }
    }
}