using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class RestaurantQueryDTO : PageDTO
    {
        [StringLength(150)]
        public string? Search { get; set; }
        [StringLength(100)]
        public string? City { get; set; }
        public bool? IsActive { get; set; }
    }
}