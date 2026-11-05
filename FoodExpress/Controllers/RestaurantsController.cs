using FoodExpress.Core.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using FoodExpress.Responses;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Core.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FoodExpress.Controllers
{
    [ApiController]
    [Route("api/restaurants")]
    public class RestaurantsController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public RestaurantsController(IRestaurantService service)
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

            var url = await images.UploadAsync(id, true, stream, image.FileName, image.ContentType, ct);

            return Ok(ApiResponse<string>.Ok(url));
        }

        /// <summary>
        /// Returns restaurants with search, city, active status, and pagination filters.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<RestaurantDTO>>>> List([FromQuery] RestaurantQueryDTO query, CancellationToken ct)
        {
            var result = await _service.ListAsync(query, ct);

            var response = ApiResponse<IReadOnlyList<RestaurantDTO>>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Returns a restaurant by ID.
        /// </summary>
        /// <param name="id">The restaurant ID.</param>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<RestaurantDTO>>> Get(int id, CancellationToken ct)
        {
            var result = await _service.GetAsync(id, ct);

            var response = ApiResponse<RestaurantDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Creates a restaurant. Requires the Admin role.
        /// </summary>
        /// <param name="request">The new restaurant details.</param>
        [HttpPost, Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<ApiResponse<RestaurantDTO>>> Create(CreateRestaurantDTO request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(request, ct);

            var response = ApiResponse<RestaurantDTO>.Ok(result);

            return CreatedAtAction(nameof(Get), new { id = result.Id }, response);
        }

        /// <summary>
        /// Updates a restaurant. Requires the Admin role.
        /// </summary>
        /// <param name="id">The restaurant ID.</param>
        /// <param name="request">The updated restaurant details.</param>
        [HttpPut("{id:int}"), Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<ApiResponse<RestaurantDTO>>> Update(int id, UpdateRestaurantDTO request, CancellationToken ct)
        {
            var result = await _service.UpdateAsync(id, request, ct);

            var response = ApiResponse<RestaurantDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Deletes a restaurant. Requires the Admin role.
        /// </summary>
        /// <param name="id">The restaurant ID.</param>
        [HttpDelete("{id:int}"), Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);

            return NoContent();
        }
    }
}