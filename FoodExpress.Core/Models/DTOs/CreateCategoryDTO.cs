using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class CreateCategoryDTO
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
