using FoodExpress.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Responses;
using FoodExpress.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoodExpress.Controllers
{
    [ApiController, Authorize]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly ICurrentUser _current;

        public UsersController(IUserService service, ICurrentUser current)
        {
            _service = service;
            _current = current;
        }

        /// <summary>
        /// Returns the current user's profile.
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Get(CancellationToken ct)
        {
            var result = await _service.GetAsync(_current.UserId, ct);

            var response = ApiResponse<UserDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Updates the current user's profile.
        /// </summary>
        /// <param name="dto">The updated profile details.</param>
        [HttpPut("me")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Update(UpdateUserDTO dto, CancellationToken ct)
        {
            var result = await _service.UpdateAsync(_current.UserId, dto, ct);

            var response = ApiResponse<UserDTO>.Ok(result, "Profile updated.");

            return Ok(response);
        }

        /// <summary>
        /// Changes the current user's password.
        /// </summary>
        /// <param name="dto">The current password and new password.</param>
        [HttpPost("me/change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto, CancellationToken ct)
        {
            await _service.ChangePasswordAsync(_current.UserId, dto, ct);

            return NoContent();
        }
    }
}