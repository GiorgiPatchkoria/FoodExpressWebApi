using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;
    }
}