namespace FoodExpress.Core.Models.Entities
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public int RestaurantId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public Restaurant Restaurant { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}