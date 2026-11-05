using AutoMapper;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Services.Profiles
{
    public class RestaurantProfile : Profile
    {
        public RestaurantProfile()
        {
            CreateMap<Restaurant, RestaurantDTO>();
            CreateMap<CreateRestaurantDTO, Restaurant>(MemberList.None);
            CreateMap<UpdateRestaurantDTO, Restaurant>(MemberList.None);
        }
    }
}