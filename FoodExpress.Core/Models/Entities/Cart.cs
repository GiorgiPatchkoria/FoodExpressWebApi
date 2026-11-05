using FoodExpress.Core.Models.Identity;

namespace FoodExpress.Core.Models.Entities
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}