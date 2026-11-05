namespace FoodExpress.Core.Models.DTOs
{
    public class AddressDTO
    {
        public int Id { get; set; }
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string CountryCode { get; set; } = string.Empty;
    }
}