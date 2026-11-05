using AutoMapper;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Services.Profiles
{
    public class MenuItemProfile : Profile
    {
        public MenuItemProfile()
        {
            CreateMap<MenuItem, MenuItemDTO>();
            CreateMap<CreateMenuItemDTO, MenuItem>(MemberList.None);
            CreateMap<UpdateMenuItemDTO, MenuItem>(MemberList.None);
        }
    }
}