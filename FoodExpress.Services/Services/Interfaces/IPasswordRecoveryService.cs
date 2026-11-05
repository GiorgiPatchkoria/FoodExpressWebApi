using FoodExpress.Core.Models.DTOs;

namespace FoodExpress.Services.Services.Interfaces
{
    public interface IPasswordRecoveryService
    {
        Task ForgotAsync(ForgotPasswordDTO dto, CancellationToken ct = default);
        Task ResetAsync(ResetPasswordDTO dto, CancellationToken ct = default);
    }
}