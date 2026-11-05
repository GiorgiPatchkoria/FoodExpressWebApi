using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class MenuQueryDTO : PageDTO, IValidatableObject
    {
        [StringLength(150)]
        public string? Search { get; set; }
        [Range(1, int.MaxValue)]
        public int? CategoryId { get; set; }
        [Range(typeof(decimal), "0", "999999.99")]
        public decimal? MinPrice { get; set; }
        [Range(typeof(decimal), "0", "999999.99")]
        public decimal? MaxPrice { get; set; }
        public bool? IsAvailable { get; set; }
        [RegularExpression("^(name|price|price-desc)$")]
        public string? Sort { get; set; } = "name";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
            {
                yield return new ValidationResult("Minimum price cannot exceed maximum price.", [nameof(MinPrice), nameof(MaxPrice)]);
            }
        }
    }
}