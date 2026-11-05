using FoodExpress.Core.Interfaces;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;
using AutoMapper;

namespace FoodExpress.Services.Services.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _restaurants;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestValidator _validator;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _current;

        public RestaurantService(IRestaurantRepository restaurants, IUnitOfWork unitOfWork, IRequestValidator validator, IMapper mapper, ICurrentUser current)
        {
            _restaurants = restaurants;
            _unitOfWork = unitOfWork;
            _validator = validator;
            _mapper = mapper;
            _current = current;
        }

        public async Task<RestaurantDTO> GetAsync(int id, CancellationToken ct = default)
        {
            return _mapper.Map<RestaurantDTO>(await _restaurants.FindAsync(id, ct) ?? throw NotFound());
        }

        public async Task<IReadOnlyList<RestaurantDTO>> ListAsync(RestaurantQueryDTO query, CancellationToken ct = default)
        {
            _validator.Validate(query);
            return (await _restaurants.ListAsync(query, ct)).Select(x => _mapper.Map<RestaurantDTO>(x)).ToList();
        }

        public async Task<RestaurantDTO> CreateAsync(CreateRestaurantDTO request, CancellationToken ct = default)
        {
            ServiceAccess.RequireAdmin(_current);
            _validator.Validate(request);
            var restaurant = new Restaurant();
            request.Name = request.Name.Trim();
            request.StreetAddress = request.StreetAddress.Trim();
            request.City = request.City.Trim();
            request.PhoneNumber = request.PhoneNumber.Trim();
            _mapper.Map(request, restaurant);
            await _restaurants.AddAsync(restaurant, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<RestaurantDTO>(restaurant);
        }

        public async Task<RestaurantDTO> UpdateAsync(int id, UpdateRestaurantDTO request, CancellationToken ct = default)
        {
            _validator.PositiveId(id);
            ServiceAccess.RequireAdmin(_current);
            _validator.Validate(request);
            var restaurant = await _restaurants.FindAsync(id, ct) ?? throw NotFound();
            request.Name = request.Name.Trim();
            request.StreetAddress = request.StreetAddress.Trim();
            request.City = request.City.Trim();
            request.PhoneNumber = request.PhoneNumber.Trim();
            _mapper.Map(request, restaurant);
            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<RestaurantDTO>(restaurant);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            ServiceAccess.RequireAdmin(_current);
            _validator.PositiveId(id);
            var entity = await _restaurants.FindAsync(id, ct) ?? throw NotFound();
            _restaurants.Remove(entity);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        private static ServiceException NotFound()
        {
            return new(ServiceError.NotFound, "Restaurant not found.");
        }
    }
}