using System.Text.Json.Serialization;

namespace FoodExpress.Core.Models.DTOs
{
    public class OrderItemDTO
    {
        public int MenuItemId { get; set; }
        [JsonPropertyName("name")]
        public string ItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal
        {
            get { return UnitPrice * Quantity; }
        }

    }
}