using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class CreateMenuItemDTO
    {
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;
        [StringLength(2000)]
        public string? Description { get; set; }
        [Required, Range(typeof(decimal), "0", "999999.99")]
        public decimal? Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        [Range(1, int.MaxValue)]
        public int RestaurantId { get; set; }
    }
}