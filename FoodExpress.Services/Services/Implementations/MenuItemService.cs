using FoodExpress.Core.Interfaces;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;
using AutoMapper;

namespace FoodExpress.Services.Services.Implementations
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IMenuItemRepository _menu;
        private readonly ICategoryRepository _categories;
        private readonly IRestaurantRepository _restaurants;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestValidator _validator;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _current;

        public MenuItemService(IMenuItemRepository menu, ICategoryRepository categories, IRestaurantRepository restaurants, IUnitOfWork unitOfWork, IRequestValidator validator, IMapper mapper, ICurrentUser current)
        {
            _menu = menu;
            _categories = categories;
            _restaurants = restaurants;
            _unitOfWork = unitOfWork;
            _validator = validator;
            _mapper = mapper;
            _current = current;
        }

        public async Task<MenuItemDTO> GetAsync(int id, CancellationToken ct = default)
        {
            _validator.PositiveId(id);
            return _mapper.Map<MenuItemDTO>(await _menu.FindAsync(id, ct) ?? throw NotFound());
        }

        public async Task<IReadOnlyList<MenuItemDTO>> ListAsync(int? restaurantId, MenuQueryDTO query, CancellationToken ct = default)
        {
            if (restaurantId.HasValue)
            {
                _validator.PositiveId(restaurantId.Value);
            }

            _validator.Validate(query);
            if (restaurantId.HasValue && await _restaurants.FindAsync(restaurantId.Value, ct) is null)
            {
                throw new ServiceException(ServiceError.NotFound, "Restaurant not found.");
            }

            return (await _menu.ListAsync(restaurantId, query, ct)).Select(x => _mapper.Map<MenuItemDTO>(x)).ToList();
        }

        public async Task<MenuItemDTO> CreateAsync(CreateMenuItemDTO request, CancellationToken ct = default)
        {
            ServiceAccess.RequireAdmin(_current);
            _validator.Validate(request);
            if (await _restaurants.FindAsync(request.RestaurantId, ct) is null)
            {
                throw new ServiceException(ServiceError.NotFound, "Restaurant not found.");
            }

            await CheckReferences(request.CategoryId, request.Name, request.RestaurantId, null, ct);
            var item = new MenuItem { RestaurantId = request.RestaurantId };
            request.Name = request.Name.Trim();
            request.Description = request.Description?.Trim();
            _mapper.Map(request, item);
            await _menu.AddAsync(item, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<MenuItemDTO>(item);
        }

        public async Task<MenuItemDTO> UpdateAsync(int id, UpdateMenuItemDTO request, CancellationToken ct = default)
        {
            _validator.PositiveId(id);
            ServiceAccess.RequireAdmin(_current);
            _validator.Validate(request);
            var item = await _menu.FindAsync(id, ct) ?? throw NotFound();
            await CheckReferences(request.CategoryId, request.Name, item.RestaurantId, id, ct);
            request.Name = request.Name.Trim();
            request.Description = request.Description?.Trim();
            _mapper.Map(request, item);
            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<MenuItemDTO>(item);
        }

        private async Task CheckReferences(int categoryId, string name, int restaurantId, int? id, CancellationToken ct)
        {
            if (!await _categories.CategoryExistsAsync(categoryId, ct))
            {
                throw new ServiceException(ServiceError.NotFound, "Category not found.");
            }

            if (await _menu.NameExistsAsync(restaurantId, name.Trim(), id, ct))
            {
                throw new ServiceException(ServiceError.Conflict, "This restaurant already has a menu item with that name.");
            }
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            ServiceAccess.RequireAdmin(_current);
            _validator.PositiveId(id);
            var entity = await _menu.FindAsync(id, ct) ?? throw NotFound();
            _menu.Remove(entity);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        private static ServiceException NotFound()
        {
            return new(ServiceError.NotFound, "Menu item not found.");
        }
    }
}