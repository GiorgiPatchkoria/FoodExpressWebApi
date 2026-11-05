using AutoMapper;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Identity;

namespace FoodExpress.Services.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserDTO>()
                .ForMember(dto => dto.Role, options => options.MapFrom(user =>
                    user.RoleId == Roles.AdminId ? Roles.Admin : Roles.Customer));
        }
    }
}