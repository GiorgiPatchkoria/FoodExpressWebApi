using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class UpdateUserDTO
    {
        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;
        [Required, StringLength(50, MinimumLength = 3), RegularExpression("[a-zA-Z0-9_.-]+")]
        public string UserName { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;
        [Phone, StringLength(30)]
        public string? PhoneNumber { get; set; }
        [StringLength(128)]
        public string? CurrentPassword { get; set; }
    }
}