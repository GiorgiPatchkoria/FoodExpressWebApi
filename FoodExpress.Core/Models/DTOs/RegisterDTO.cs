using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class RegisterDTO
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;
        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;
        [Phone, StringLength(30)]
        public string? PhoneNumber { get; set; }
        [Required, StringLength(50, MinimumLength = 3), RegularExpression("[a-zA-Z0-9_.-]+")]
        public string UserName { get; set; } = string.Empty;
        [Required, StringLength(128, MinimumLength = 12)]
        public string Password { get; set; } = string.Empty;
    }
}