using FoodExpress.Core.Interfaces;
using FoodExpress.Infrastructure.Data;

namespace FoodExpress.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FoodDbContext _context;

        public UnitOfWork(FoodDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
