using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class UpdateRestaurantDTO
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;
        [Required, StringLength(250)]
        public string StreetAddress { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;
        [Required, Phone, StringLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}