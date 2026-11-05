using FoodExpress.Core.Models.DTOs;
using FoodExpress.Responses;
using FoodExpress.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodExpress.Controllers
{
    [ApiController, AllowAnonymous]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly IPasswordRecoveryService _passwordRecovery;

        public AuthController(IAuthService service, IPasswordRecoveryService passwordRecovery)
        {
            _service = service;
            _passwordRecovery = passwordRecovery;
        }

        /// <summary>
        /// Registers a user and returns authentication details.
        /// </summary>
        /// <param name="request">The user registration details.</param>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthenticationDTO>>> Register(RegisterDTO request, CancellationToken ct)
        {
            var result = await _service.RegisterAsync(request, ct);

            var response = ApiResponse<AuthenticationDTO>.Ok(result);

            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>
        /// Validates the email and password and returns a JWT token.
        /// </summary>
        /// <param name="request">The user's email and password.</param>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthenticationDTO>>> Login(LoginDTO request, CancellationToken ct)
        {
            var result = await _service.LoginAsync(request, ct);

            var response = ApiResponse<AuthenticationDTO>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Sends a password reset link to the registered email address.
        /// </summary>
        /// <param name="dto">The email address to receive the password reset link.</param>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<string>>> ForgotPassword(ForgotPasswordDTO dto, CancellationToken ct)
        {
            await _passwordRecovery.ForgotAsync(dto, ct);
            var response = ApiResponse<string>.Ok("If an account exists for that email, a password reset link will be sent. Check your inbox and spam folder.");
            return Ok(response);
        }

        /// <summary>
        /// Sets a new password using a valid reset token.
        /// </summary>
        /// <param name="dto">The user's email, reset token, and new password.</param>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto, CancellationToken ct)
        {
            await _passwordRecovery.ResetAsync(dto, ct);
            return NoContent();
        }
    }
}