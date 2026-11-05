using FoodExpress.Core.Enums;
namespace FoodExpress.Core.Models.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Subtotal
        {
            get { return TotalPrice; }
        }
        public decimal TotalPrice { get; set; }

    }
}