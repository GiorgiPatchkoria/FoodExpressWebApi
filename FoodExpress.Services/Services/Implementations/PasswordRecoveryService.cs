using System.Text;
using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Identity;
using FoodExpress.Services.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace FoodExpress.Services.Services.Implementations
{
    public class PasswordRecoveryService : IPasswordRecoveryService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRequestValidator _validator;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordRecoveryService> _logger;

        public PasswordRecoveryService(UserManager<ApplicationUser> userManager, IRequestValidator validator,
            ILogger<PasswordRecoveryService> logger, IEmailService emailService)
        {
            _userManager = userManager;
            _validator = validator;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task ForgotAsync(ForgotPasswordDTO dto, CancellationToken ct = default)
        {
            _validator.Validate(dto);
            var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
            if (user == null)
            {
                return;
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetPath = "/#/reset-password?email=" + Uri.EscapeDataString(user.Email!) + "&token=" + Uri.EscapeDataString(encodedToken);
            try
            {
                _emailService.SendPasswordResetEmail(user.Email!, resetPath);
            }
            catch (InvalidOperationException)
            {
                _logger.LogError("Password recovery email delivery failed. Check SMTP configuration and availability.");
            }
        }

        public async Task ResetAsync(ResetPasswordDTO dto, CancellationToken ct = default)
        {
            _validator.Validate(dto);
            var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
            if (user == null)
            {
                throw new ServiceException(ServiceError.Validation, "The reset link is invalid or has expired. Request a new one.");
            }
            string token;
            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
            }
            catch (FormatException)
            {
                throw new ServiceException(ServiceError.Validation, "The reset link is invalid or has expired. Request a new one.");
            }
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!result.Succeeded)
            {
                throw new ServiceException(ServiceError.Validation, "The reset link is invalid or has expired, or the new password does not meet the requirements. Request a new link and use 12–128 characters.");
            }
        }
    }
}