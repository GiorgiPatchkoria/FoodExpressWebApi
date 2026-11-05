using AutoMapper;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Services.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderItem, OrderItemDTO>();
            CreateMap<Order, OrderDTO>();
            CreateMap<Order, OrderDetailsDTO>()
                .ForMember(d => d.Order, o => o.MapFrom(s => s))
                .ForMember(d => d.DeliveryAddress, o => o.MapFrom(s => s.Address))
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.OrderBy(i => i.MenuItemId)));
        }
    }
}