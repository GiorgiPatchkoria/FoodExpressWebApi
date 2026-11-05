using AutoMapper;
using FoodExpress.Core.Interfaces;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Services.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _carts;
        private readonly IMenuItemRepository _menu;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IRequestValidator _validator;
        private readonly IMapper _mapper;

        public CartService(ICartRepository carts, IMenuItemRepository menu, IUnitOfWork unitOfWork, IUserService userService, IRequestValidator validator, IMapper mapper)
        {
            _carts = carts;
            _menu = menu;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<CartDTO> GetAsync(int userId, CancellationToken ct = default)
        {
            await _userService.GetAsync(userId, ct);
            return _mapper.Map<CartDTO>(await _carts.FindByUserAsync(userId, ct) ?? new Cart());
        }

        public async Task<CartDTO> AddItemAsync(int userId, AddCartItemDTO request, CancellationToken ct = default)
        {
            await _userService.GetAsync(userId, ct);
            _validator.Validate(request);
            var cart = await _carts.FindByUserAsync(userId, ct);
            var item = await _menu.FindAsync(request.MenuItemId, ct) ?? throw new ServiceException(ServiceError.NotFound, "Menu item not found.");
            EnsureAvailable(item);
            if (cart is null)
            {
                cart = new Cart { UserId = userId };
                await _carts.AddAsync(cart, ct);
            }
            if (cart.Items.Any(x => x.MenuItem.RestaurantId != item.RestaurantId))
            {
                throw new ServiceException(ServiceError.Conflict, "A cart may contain items from only one restaurant.");
            }

            var line = cart.Items.SingleOrDefault(x => x.MenuItemId == item.Id);
            var quantity = (line?.Quantity ?? 0) + request.Quantity;
            if (quantity > 99)
            {
                throw new ServiceException(ServiceError.Validation, "A cart line cannot exceed 99 items.");
            }

            if (line is null)
            {
                cart.Items.Add(new CartItem { MenuItemId = item.Id, MenuItem = item, Quantity = quantity });
            }
            else
            {
                line.Quantity = quantity;
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<CartDTO>(cart);
        }

        public async Task<CartDTO> UpdateItemAsync(int userId, int menuItemId, UpdateCartItemDTO request, CancellationToken ct = default)
        {
            await _userService.GetAsync(userId, ct);
            _validator.PositiveId(menuItemId);
            _validator.Validate(request);
            var cart = await _carts.FindByUserAsync(userId, ct);
            var line = cart?.Items.SingleOrDefault(x => x.MenuItemId == menuItemId) ?? throw new ServiceException(ServiceError.NotFound, "Cart item not found.");
            EnsureAvailable(line.MenuItem);
            line.Quantity = request.Quantity;
            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<CartDTO>(cart);
        }

        public async Task<CartDTO> RemoveItemAsync(int userId, int menuItemId, CancellationToken ct = default)
        {
            await _userService.GetAsync(userId, ct);
            _validator.PositiveId(menuItemId);
            var cart = await _carts.FindByUserAsync(userId, ct);
            var line = cart?.Items.SingleOrDefault(x => x.MenuItemId == menuItemId) ?? throw new ServiceException(ServiceError.NotFound, "Cart item not found.");
            _carts.RemoveItem(line);
            cart!.Items.Remove(line);
            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<CartDTO>(cart);
        }

        internal static void EnsureAvailable(MenuItem item)
        {
            if (!item.IsAvailable || !item.Restaurant.IsActive)
            {
                throw new ServiceException(ServiceError.Conflict, "One or more menu items are unavailable.");
            }
        }

    }
}