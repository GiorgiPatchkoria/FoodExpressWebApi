using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class ResetPasswordDTO
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;
        [Required, StringLength(4096)]
        public string Token { get; set; } = string.Empty;
        [Required, StringLength(128, MinimumLength = 12)]
        public string NewPassword { get; set; } = string.Empty;
        [Required, Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}