using System.Text.Json.Serialization;

namespace FoodExpress.Core.Models.DTOs
{
    public class CartItemDTO
    {
        public int MenuItemId { get; set; }
        [JsonPropertyName("restaurantId")]
        public int MenuItemRestaurantId { get; set; }
        [JsonPropertyName("name")]
        public string MenuItemName { get; set; } = string.Empty;
        [JsonPropertyName("unitPrice")]
        public decimal MenuItemPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsAvailable { get; set; }
        public decimal LineTotal
        {
            get { return MenuItemPrice * Quantity; }
        }

    }
}