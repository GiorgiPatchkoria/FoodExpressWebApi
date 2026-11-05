using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using FoodExpress.Responses;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Core.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FoodExpress.Controllers
{
    [ApiController, Authorize]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;
        private readonly ICurrentUser _current;

        public OrdersController(IOrderService service, ICurrentUser current)
        {
            _service = service;
            _current = current;
        }

        /// <summary>
        /// Creates an order from the current user's cart and calculates its total.
        /// </summary>
        /// <param name="request">The delivery address ID for the order.</param>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDetailsDTO>>> Create(CreateOrderDTO request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(_current.UserId, request, ct);

            var response = ApiResponse<OrderDetailsDTO>.Ok(result);

            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>
        /// Returns the current user's paginated order history.
        /// </summary>
        [HttpGet("my")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderDTO>>>> List([FromQuery] PageDTO pagination, CancellationToken ct)
        {
            var result = await _service.ListAsync(_current.UserId, pagination, ct);

            var response = ApiResponse<IReadOnlyList<OrderDTO>>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Returns order details for the order's owner or an administrator.
        /// </summary>
        /// <param name="id">The order ID.</param>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<OrderDetailsDTO>>> Get(int id, CancellationToken ct)
        {
            var result = await _service.GetAsync(id, ct);

            var response = ApiResponse<OrderDetailsDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Updates the order status using an allowed transition. Requires the Admin role.
        /// </summary>
        /// <param name="id">The order ID.</param>
        /// <param name="request">The new order status.</param>
        [HttpPatch("{id:int}/status"), Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<ApiResponse<OrderDetailsDTO>>> UpdateStatus(int id, UpdateOrderStatusDTO request, CancellationToken ct)
        {
            var result = await _service.UpdateStatusAsync(id, request, ct);

            var response = ApiResponse<OrderDetailsDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Cancels an accessible order only when its status is Created or Confirmed.
        /// </summary>
        /// <param name="id">The order ID.</param>
        [HttpPatch("{id:int}/cancel")]
        public async Task<ActionResult<ApiResponse<OrderDetailsDTO>>> Cancel(int id, CancellationToken ct)
        {
            var result = await _service.CancelAsync(id, ct);

            var response = ApiResponse<OrderDetailsDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Returns order statistics grouped by status. Requires the Admin role.
        /// </summary>
        [HttpGet("summary"), Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderStatusSummaryDTO>>>> Summary(CancellationToken ct)
        {
            var result = await _service.SummaryAsync(ct);

            var response = ApiResponse<IReadOnlyList<OrderStatusSummaryDTO>>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Returns a paginated list of all users' orders. Requires the Admin role.
        /// </summary>
        [HttpGet, Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderDTO>>>> ListAll([FromQuery] PageDTO pagination, CancellationToken ct)
        {
            var result = await _service.ListAllAsync(pagination, ct);

            var response = ApiResponse<IReadOnlyList<OrderDTO>>.Ok(result);

            return Ok(response);
        }
    }
}