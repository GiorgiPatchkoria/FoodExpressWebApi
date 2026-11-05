using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Core.Interfaces
{
    public interface IAddressRepository
    {
        Task<Address?> FindForUserAsync(int id, int userId, CancellationToken ct);
        Task<IReadOnlyList<Address>> ListByUserAsync(int userId, CancellationToken ct);
        Task AddAsync(Address address, CancellationToken ct = default);
    }
}