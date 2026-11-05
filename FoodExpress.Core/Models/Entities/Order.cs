using FoodExpress.Core.Enums;
using FoodExpress.Core.Models.Identity;

namespace FoodExpress.Core.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AddressId { get; set; }
        public Address Address { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        public int RestaurantId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        public ApplicationUser User { get; set; } = null!;
        public Restaurant Restaurant { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}