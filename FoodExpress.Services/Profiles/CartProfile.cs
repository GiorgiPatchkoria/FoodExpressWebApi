using AutoMapper;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Services.Profiles
{
    public class CartProfile : Profile
    {
        public CartProfile()
        {
            CreateMap<CartItem, CartItemDTO>()
                .ForMember(d => d.IsAvailable, o => o.MapFrom(s => s.MenuItem.IsAvailable && s.MenuItem.Restaurant.IsActive));
            CreateMap<Cart, CartDTO>()
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.OrderBy(i => i.MenuItemId)));
        }
    }
}