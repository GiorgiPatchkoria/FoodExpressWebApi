namespace FoodExpress.Core.Models.DTOs
{
    public class AuthenticationDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public UserDTO User { get; set; } = null!;
    }
}