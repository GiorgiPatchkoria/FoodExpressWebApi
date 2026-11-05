using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.Entities;
using FoodExpress.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly FoodDbContext _context;

        public CategoryRepository(FoodDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Category>> ListAsync(int skip, int take, CancellationToken ct = default)
        {
            return await _context.Categories
                .OrderBy(category => category.Name).ThenBy(category => category.Id)
                .Skip(skip).Take(take).ToListAsync(ct);
        }

        public async Task<Category?> FindAsync(int id, CancellationToken ct = default)
        {
            return await _context.Categories.SingleOrDefaultAsync(category => category.Id == id, ct);
        }

        public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
        {
            return await _context.Categories.AnyAsync(category => category.Name == name, ct);
        }

        public Task<bool> CategoryExistsAsync(int id, CancellationToken ct)
        {
            return _context.Categories.AnyAsync(category => category.Id == id, ct);
        }

        public async Task AddAsync(Category category, CancellationToken ct = default)
        {
            await _context.Categories.AddAsync(category, ct);
        }
    }
}