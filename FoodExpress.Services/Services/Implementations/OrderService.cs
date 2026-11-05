using FoodExpress.Core.Enums;
using AutoMapper;
using FoodExpress.Core.Interfaces;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;

namespace FoodExpress.Services.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orders;
        private readonly ICartRepository _carts;
        private readonly IAddressRepository _addresses;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IRequestValidator _validator;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _current;

        public OrderService(IOrderRepository orders, ICartRepository carts, IAddressRepository addresses, IUnitOfWork unitOfWork, IUserService userService, IRequestValidator validator, ICurrentUser current, IMapper mapper)
        {
            _orders = orders;
            _carts = carts;
            _addresses = addresses;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _validator = validator;
            _mapper = mapper;
            _current = current;
        }

        public async Task<OrderDetailsDTO> CreateAsync(int userId, CreateOrderDTO request, CancellationToken ct = default)
        {
            await _userService.GetAsync(userId, ct);
            _validator.Validate(request);
            var address = await _addresses.FindForUserAsync(request.AddressId, userId, ct) ?? throw new ServiceException(ServiceError.NotFound, "Address not found.");
            var cart = await _carts.FindByUserAsync(userId, ct);
            if (cart is null || cart.Items.Count == 0)
            {
                throw new ServiceException(ServiceError.Validation, "The cart is empty.");
            }

            foreach (var line in cart.Items)
            {
                CartService.EnsureAvailable(line.MenuItem);
                if (line.Quantity is < 1 or > 99)
                {
                    throw new ServiceException(ServiceError.Validation, "Cart quantities must be between 1 and 99.");
                }
            }
            if (cart.Items.Select(x => x.MenuItem.RestaurantId).Distinct().Count() != 1)
            {
                throw new ServiceException(ServiceError.Conflict, "An order must contain items from one restaurant.");
            }

            var order = new Order
            {
                UserId = userId,
                RestaurantId = cart.Items.First().MenuItem.RestaurantId,
                AddressId = address.Id,
                Address = address,
                Status = OrderStatus.Created,
                Items = cart.Items.Select(x => new OrderItem
                {
                    MenuItemId = x.MenuItemId,
                    ItemName = x.MenuItem.Name,
                    UnitPrice = x.MenuItem.Price,
                    Quantity = x.Quantity
                }).ToList()
            };
            order.TotalPrice = order.Items.Sum(x => x.UnitPrice * x.Quantity);
            await _orders.AddAsync(order, ct);
            foreach (var line in cart.Items.ToList())
            {
                _carts.RemoveItem(line);
            }

            cart.Items.Clear();
            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<OrderDetailsDTO>(order);
        }

        public async Task<IReadOnlyList<OrderDTO>> ListAsync(int userId, PageDTO page, CancellationToken ct = default)
        {
            await _userService.GetAsync(userId, ct);
            _validator.Validate(page);
            return (await _orders.ListByUserAsync(userId, (page.Page - 1) * page.PageSize, page.PageSize, ct)).Select(x => _mapper.Map<OrderDTO>(x)).ToList();
        }

        public async Task<OrderDetailsDTO> GetAsync(int id, CancellationToken ct = default)
        {
            ServiceAccess.RequireUser(_current);
            _validator.PositiveId(id);
            return _mapper.Map<OrderDetailsDTO>(await FindOrder(id, ct));
        }

        public async Task<OrderDetailsDTO> UpdateStatusAsync(int id, UpdateOrderStatusDTO request, CancellationToken ct = default)
        {
            ServiceAccess.RequireAdmin(_current);
            _validator.PositiveId(id);
            _validator.Validate(request);
            var order = await FindOrder(id, ct);
            var next = request.Status!.Value;
            if (!CanTransition(order.Status, next))
            {
                throw new ServiceException(ServiceError.Conflict, "The requested order status transition is not allowed.");
            }

            order.Status = next;
            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<OrderDetailsDTO>(order);
        }

        public async Task<OrderDetailsDTO> CancelAsync(int id, CancellationToken ct = default)
        {
            ServiceAccess.RequireUser(_current);
            _validator.PositiveId(id);
            var order = await FindOrder(id, ct);
            ServiceAccess.RequireOwner(_current, order.UserId);
            if (!CanTransition(order.Status, OrderStatus.Cancelled))
            {
                throw new ServiceException(ServiceError.Conflict, "Only Created or Confirmed orders can be cancelled.");
            }

            order.Status = OrderStatus.Cancelled;
            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<OrderDetailsDTO>(order);
        }

        public Task<IReadOnlyList<OrderStatusSummaryDTO>> SummaryAsync(CancellationToken ct = default)
        {
            ServiceAccess.RequireAdmin(_current);
            return _orders.GetStatusSummaryAsync(ct);
        }

        private async Task<Order> FindOrder(int id, CancellationToken ct)
        {
            var order = await _orders.FindAsync(id, ct);
            if (order is null || (order.UserId != _current.UserId && !_current.IsAdmin))
            {
                throw new ServiceException(ServiceError.NotFound, "Order not found.");
            }

            return order;
        }

        public async Task<IReadOnlyList<OrderDTO>> ListAllAsync(PageDTO page, CancellationToken ct = default)
        {
            ServiceAccess.RequireAdmin(_current);
            _validator.Validate(page);
            return (await _orders.ListAllAsync((page.Page - 1) * page.PageSize, page.PageSize, ct)).Select(x => _mapper.Map<OrderDTO>(x)).ToList();
        }

        private static bool CanTransition(OrderStatus from, OrderStatus to)
        {
            return (from, to) switch
            {
                (OrderStatus.Created, OrderStatus.Confirmed or OrderStatus.Cancelled) => true,
                (OrderStatus.Confirmed, OrderStatus.Preparing or OrderStatus.Cancelled) => true,
                (OrderStatus.Preparing, OrderStatus.OnTheWay) => true,
                (OrderStatus.OnTheWay, OrderStatus.Delivered) => true,
                _ => false
            };
        }
    }
}