using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class CreateOrderDTO
    {
        [Range(1, int.MaxValue)]
        public int AddressId { get; set; }
    }
}