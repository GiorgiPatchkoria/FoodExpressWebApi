using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class LoginDTO
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;
        [Required, StringLength(128)]
        public string Password { get; set; } = string.Empty;
    }
}