using FoodExpress.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Core.Models.DTOs
{
    public class UpdateOrderStatusDTO
    {
        [Required, EnumDataType(typeof(OrderStatus))]
        public OrderStatus? Status { get; set; }
    }
}