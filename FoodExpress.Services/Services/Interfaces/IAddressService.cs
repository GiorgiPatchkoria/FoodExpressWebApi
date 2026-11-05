using FoodExpress.Core.Models.DTOs;

namespace FoodExpress.Services.Services.Interfaces
{
    public interface IAddressService
    {
        Task<AddressDTO> CreateAsync(int userId, CreateAddressDTO request, CancellationToken ct = default);
        Task<IReadOnlyList<AddressDTO>> ListAsync(int userId, CancellationToken ct = default);
    }
}