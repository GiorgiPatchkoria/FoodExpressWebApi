using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.Entities;
using FoodExpress.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Infrastructure.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly FoodDbContext _context;

        public AddressRepository(FoodDbContext db)
        {
            _context = db;
        }

        public Task<Address?> FindForUserAsync(int id, int userId, CancellationToken ct)
        {
            return _context.Addresses.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        }

        public async Task<IReadOnlyList<Address>> ListByUserAsync(int userId, CancellationToken ct)
        {
            return await _context.Addresses.Where(x => x.UserId == userId).OrderBy(x => x.Id).ToListAsync(ct);
        }

        public async Task AddAsync(Address address, CancellationToken ct = default)
        {
            await _context.Addresses.AddAsync(address, ct);
        }
    }
}