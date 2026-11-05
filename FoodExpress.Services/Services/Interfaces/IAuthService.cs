using FoodExpress.Core.Models.DTOs;

namespace FoodExpress.Services.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthenticationDTO> RegisterAsync(RegisterDTO request, CancellationToken ct = default);
        Task<AuthenticationDTO> LoginAsync(LoginDTO request, CancellationToken ct = default);
    }
}