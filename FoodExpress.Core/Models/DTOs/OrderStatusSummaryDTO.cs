using FoodExpress.Core.Enums;

namespace FoodExpress.Core.Models.DTOs
{
    public class OrderStatusSummaryDTO
    {
        public OrderStatus Status { get; set; }
        public int Count { get; set; }
        public decimal TotalPrice { get; set; }

        public OrderStatusSummaryDTO()
        {
        }

        public OrderStatusSummaryDTO(OrderStatus status, int count, decimal totalPrice)
        {
            Status = status;
            Count = count;
            TotalPrice = totalPrice;
        }
    }
}