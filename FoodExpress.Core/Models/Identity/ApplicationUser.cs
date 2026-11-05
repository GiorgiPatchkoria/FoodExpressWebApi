using FoodExpress.Core.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace FoodExpress.Core.Models.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public int RoleId { get; set; } = Roles.CustomerId;
        public Role Role { get; set; } = null!;
        public string FullName { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}