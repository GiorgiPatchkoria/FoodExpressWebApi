namespace FoodExpress.Core.Models.DTOs
{
    public class OrderDetailsDTO
    {
        public OrderDTO Order { get; set; } = null!;
        public AddressDTO DeliveryAddress { get; set; } = null!;
        public IReadOnlyList<OrderItemDTO> Items { get; set; } = [];
    }
}