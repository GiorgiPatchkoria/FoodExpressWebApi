using FoodExpress.Core.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using FoodExpress.Responses;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Core.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FoodExpress.Controllers
{
    [ApiController]
    [Route("api/menu-items")]
    public class MenuItemsController : ControllerBase
    {
        private readonly IMenuItemService _service;

        public MenuItemsController(IMenuItemService service)
        {
            _service = service;
        }

        /// <summary>
        /// Uploads a picture. Requires the Admin role.
        /// </summary>
        [HttpPost("{id:int}/image"), Authorize(Roles = Roles.Admin)]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<ActionResult<ApiResponse<string>>> UploadImage(int id, IFormFile image, [FromServices] ICatalogImageService images, CancellationToken ct)
        {
            await using var stream = image.OpenReadStream();

            var url = await images.UploadAsync(id, false, stream, image.FileName, image.ContentType, ct);

            return Ok(ApiResponse<string>.Ok(url));
        }

        /// <summary>
        /// Returns menu items across all restaurants with optional filters and pagination. Defaults to sorting by name.
        /// </summary>
        /// <param name="restaurantId">The restaurant ID.</param>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<MenuItemDTO>>>> List([FromQuery] int? restaurantId, [FromQuery] MenuQueryDTO query, CancellationToken ct)
        {
            var result = await _service.ListAsync(restaurantId, query, ct);

            var response = ApiResponse<IReadOnlyList<MenuItemDTO>>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Returns the restaurant menu with searching, filtering, sorting, and pagination.
        /// </summary>
        /// <param name="restaurantId">The restaurant ID.</param>
        [HttpGet("/api/restaurants/{restaurantId:int}/menu")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<MenuItemDTO>>>> RestaurantMenu(int restaurantId, [FromQuery] MenuQueryDTO query, CancellationToken ct)
        {
            var result = await _service.ListAsync(restaurantId, query, ct);

            var response = ApiResponse<IReadOnlyList<MenuItemDTO>>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Returns a menu item by ID.
        /// </summary>
        /// <param name="id">The menu item ID.</param>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<MenuItemDTO>>> Get(int id, CancellationToken ct)
        {
            var result = await _service.GetAsync(id, ct);

            var response = ApiResponse<MenuItemDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Creates a menu item. Requires the Admin role.
        /// </summary>
        /// <param name="request">The new menu item details.</param>
        [HttpPost, Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<ApiResponse<MenuItemDTO>>> Create(CreateMenuItemDTO request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(request, ct);

            var response = ApiResponse<MenuItemDTO>.Ok(result);

            return CreatedAtAction(nameof(Get), new { id = result.Id }, response);
        }

        /// <summary>
        /// Updates a menu item. Requires the Admin role.
        /// </summary>
        /// <param name="id">The menu item ID.</param>
        /// <param name="request">The updated menu item details.</param>
        [HttpPut("{id:int}"), Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<ApiResponse<MenuItemDTO>>> Update(int id, UpdateMenuItemDTO request, CancellationToken ct)
        {
            var result = await _service.UpdateAsync(id, request, ct);

            var response = ApiResponse<MenuItemDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Deletes a menu item when allowed by the business rules. Requires the Admin role.
        /// </summary>
        /// <param name="id">The menu item ID.</param>
        [HttpDelete("{id:int}"), Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);

            return NoContent();
        }
    }
}