using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class ChangePasswordDTO
    {
        [Required, StringLength(128)]
        public string CurrentPassword { get; set; } = string.Empty;
        [Required, StringLength(128, MinimumLength = 12)]
        public string NewPassword { get; set; } = string.Empty;
        [Required, Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}