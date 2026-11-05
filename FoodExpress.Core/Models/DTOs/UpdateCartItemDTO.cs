using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class UpdateCartItemDTO
    {
        [Range(1, 99)]
        public int Quantity { get; set; }
    }
}