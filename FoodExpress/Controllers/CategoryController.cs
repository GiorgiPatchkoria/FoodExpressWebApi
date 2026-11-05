using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Identity;
using FoodExpress.Responses;
using FoodExpress.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodExpress.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        /// <summary>
        /// Returns a paginated list of categories.
        /// </summary>
        /// <param name="pagination">The page number and page size.</param>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryDTO>>>> List([FromQuery] PageDTO pagination, CancellationToken ct)
        {
            var result = await _service.ListAsync(pagination, ct);

            var response = ApiResponse<IReadOnlyList<CategoryDTO>>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Returns a category by ID.
        /// </summary>
        /// <param name="id">The category ID.</param>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> Get(int id, CancellationToken ct)
        {
            var result = await _service.GetAsync(id, ct);

            var response = ApiResponse<CategoryDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Creates a category. Requires the Admin role.
        /// </summary>
        /// <param name="dto">The new category details.</param>
        [HttpPost, Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> Create(CreateCategoryDTO dto, CancellationToken ct)
        {
            var result = await _service.CreateAsync(dto, ct);

            var response = ApiResponse<CategoryDTO>.Ok(result);

            return CreatedAtAction(nameof(Get), new { id = result.Id }, response);
        }
    }
}