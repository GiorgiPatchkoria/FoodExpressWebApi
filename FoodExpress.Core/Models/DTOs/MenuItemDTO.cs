namespace FoodExpress.Core.Models.DTOs
{
    public class MenuItemDTO
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}