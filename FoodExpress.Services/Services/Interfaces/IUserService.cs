using FoodExpress.Core.Models.DTOs;

namespace FoodExpress.Services.Services.Interfaces
{
    public interface IUserService
    {
        Task RegisterUserAsync(RegisterDTO dto, CancellationToken ct = default);
        Task<UserDTO> UpdateAsync(int id, UpdateUserDTO dto, CancellationToken ct = default);
        Task ChangePasswordAsync(int id, ChangePasswordDTO dto, CancellationToken ct = default);
        Task<UserDTO> GetAsync(int id, CancellationToken ct = default);
    }
}