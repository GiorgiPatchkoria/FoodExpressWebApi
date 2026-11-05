using FoodExpress.Core.Models.Identity;

namespace FoodExpress.Core.Models.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ApplicationUser User { get; set; } = null!;
    }
}