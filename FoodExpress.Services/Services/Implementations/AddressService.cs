using FoodExpress.Core.Interfaces;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;
using AutoMapper;

namespace FoodExpress.Services.Services.Implementations
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addresses;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IRequestValidator _validator;
        private readonly IMapper _mapper;

        public AddressService(IAddressRepository addresses, IUnitOfWork unitOfWork, IUserService userService, IRequestValidator validator, IMapper mapper)
        {
            _addresses = addresses;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<AddressDTO> CreateAsync(int userId, CreateAddressDTO request, CancellationToken ct = default)
        {
            await _userService.GetAsync(userId, ct);
            _validator.Validate(request);
            var address = _mapper.Map<Address>(request);
            address.UserId = userId;
            address.StreetAddress = address.StreetAddress.Trim();
            address.City = address.City.Trim();
            address.PostalCode = address.PostalCode?.Trim();
            address.CountryCode = address.CountryCode.ToUpperInvariant();
            await _addresses.AddAsync(address, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<AddressDTO>(address);
        }

        public async Task<IReadOnlyList<AddressDTO>> ListAsync(int userId, CancellationToken ct = default)
        {
            await _userService.GetAsync(userId, ct);
            return (await _addresses.ListByUserAsync(userId, ct)).Select(x => _mapper.Map<AddressDTO>(x)).ToList();
        }
    }
}