using FoodExpress.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using FoodExpress.Responses;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Core.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FoodExpress.Controllers
{
    [ApiController, Authorize]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;
        private readonly ICurrentUser _current;

        public CartController(ICartService service, ICurrentUser current)
        {
            _service = service;
            _current = current;
        }

        /// <summary>
        /// Returns the current user's cart and its server-calculated total.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartDTO>>> Get(CancellationToken ct)
        {
            var result = await _service.GetAsync(_current.UserId, ct);

            var response = ApiResponse<CartDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Adds a menu item to the current user's cart and returns the updated cart.
        /// </summary>
        /// <param name="request">The menu item ID and quantity to add to the cart.</param>
        [HttpPost("items")]
        public async Task<ActionResult<ApiResponse<CartDTO>>> Add(AddCartItemDTO request, CancellationToken ct)
        {
            var result = await _service.AddItemAsync(_current.UserId, request, ct);

            var response = ApiResponse<CartDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Changes a cart item's quantity and returns the updated cart.
        /// </summary>
        /// <param name="menuItemId">The menu item ID in the cart.</param>
        /// <param name="request">The new quantity for the cart item.</param>
        [HttpPut("items/{menuItemId:int}")]
        public async Task<ActionResult<ApiResponse<CartDTO>>> Update(int menuItemId, UpdateCartItemDTO request, CancellationToken ct)
        {
            var result = await _service.UpdateItemAsync(_current.UserId, menuItemId, request, ct);

            var response = ApiResponse<CartDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Removes an item from the current user's cart and returns the updated cart.
        /// </summary>
        /// <param name="menuItemId">The menu item ID in the cart.</param>
        [HttpDelete("items/{menuItemId:int}")]
        public async Task<ActionResult<ApiResponse<CartDTO>>> Remove(int menuItemId, CancellationToken ct)
        {
            var result = await _service.RemoveItemAsync(_current.UserId, menuItemId, ct);

            var response = ApiResponse<CartDTO>.Ok(result);

            return Ok(response);
        }
    }
}