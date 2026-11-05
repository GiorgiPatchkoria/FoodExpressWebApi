using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;
using FoodExpress.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FoodExpress.Core.Interfaces;

namespace FoodExpress.Infrastructure.Repositories
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly FoodDbContext _context;

        public MenuItemRepository(FoodDbContext db)
        {
            _context = db;
        }

        public Task<MenuItem?> FindAsync(int id, CancellationToken ct)
        {
            return _context.MenuItems.Include(x => x.Restaurant).SingleOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IReadOnlyList<MenuItem>> ListAsync(int? restaurantId, MenuQueryDTO query, CancellationToken ct)
        {
            var rows = _context.MenuItems.Include(x => x.Restaurant).AsQueryable();
            if (restaurantId.HasValue)
            {
                rows = rows.Where(x => x.RestaurantId == restaurantId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                rows = rows.Where(x => x.Name.Contains(query.Search.Trim()));
            }

            if (query.CategoryId.HasValue)
            {
                rows = rows.Where(x => x.CategoryId == query.CategoryId.Value);
            }

            if (query.MinPrice.HasValue)
            {
                rows = rows.Where(x => x.Price >= query.MinPrice.Value);
            }

            if (query.MaxPrice.HasValue)
            {
                rows = rows.Where(x => x.Price <= query.MaxPrice.Value);
            }

            if (query.IsAvailable.HasValue)
            {
                rows = rows.Where(x => x.IsAvailable == query.IsAvailable.Value);
            }

            var ordered = query.Sort switch
            {
                "price" => rows.OrderBy(x => x.Price),
                "price-desc" => rows.OrderByDescending(x => x.Price),
                _ => rows.OrderBy(x => x.Name)
            };
            return await ordered.ThenBy(x => x.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        }

        public void Remove(MenuItem entity)
        {
            _context.MenuItems.Remove(entity);
        }

        public Task<bool> NameExistsAsync(int restaurantId, string name, int? excludingId, CancellationToken ct)
        {
            return _context.MenuItems.AnyAsync(x => x.RestaurantId == restaurantId && x.Name == name && (!excludingId.HasValue || x.Id != excludingId.Value), ct);
        }

        public async Task AddAsync(MenuItem item, CancellationToken ct = default)
        {
            await _context.MenuItems.AddAsync(item, ct);
        }
    }
}