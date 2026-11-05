namespace FoodExpress.Core.Models.DTOs
{
    public class RestaurantDTO
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}