using AutoMapper;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Services.Profiles
{
    public class AddressProfile : Profile
    {
        public AddressProfile()
        {
            CreateMap<Address, AddressDTO>();
            CreateMap<CreateAddressDTO, Address>(MemberList.None);
        }
    }
}