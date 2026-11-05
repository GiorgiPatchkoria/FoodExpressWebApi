using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class CreateAddressDTO
    {
        [Required, StringLength(250)]
        public string StreetAddress { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;
        [StringLength(20)]
        public string? PostalCode { get; set; }
        [Required, RegularExpression("[A-Za-z]{2}")]
        public string CountryCode { get; set; } = string.Empty;
    }
}