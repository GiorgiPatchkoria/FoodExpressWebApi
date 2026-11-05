namespace FoodExpress.Core.Models.DTOs
{
    public class CartDTO
    {
        public IReadOnlyList<CartItemDTO> Items { get; set; } = [];
        public decimal Subtotal
        {
            get { return Items.Sum(item => item.LineTotal); }
        }

    }
}