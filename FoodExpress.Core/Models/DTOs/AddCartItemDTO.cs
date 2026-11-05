using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class AddCartItemDTO
    {
        [Range(1, int.MaxValue)]
        public int MenuItemId { get; set; }
        [Range(1, 99)]
        public int Quantity { get; set; }
    }
}