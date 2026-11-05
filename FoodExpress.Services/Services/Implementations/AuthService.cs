using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Identity;
using FoodExpress.Services.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FoodExpress.Services.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly IRequestValidator _validator;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, IUserService userService, IRequestValidator validator, IConfiguration configuration, IMapper mapper)
        {
            _userManager = userManager;
            _userService = userService;
            _validator = validator;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<AuthenticationDTO> RegisterAsync(RegisterDTO request, CancellationToken ct = default)
        {
            await _userService.RegisterUserAsync(request, ct);
            var user = await _userManager.FindByEmailAsync(request.Email.Trim())
                ?? throw new ServiceException(ServiceError.NotFound, "User not found.");
            return GenerateJwtToken(user);
        }

        public async Task<AuthenticationDTO> LoginAsync(LoginDTO request, CancellationToken ct = default)
        {
            _validator.Validate(request);
            var user = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new ServiceException(ServiceError.Unauthorized, "Invalid email or password.");
            }

            return GenerateJwtToken(user);
        }

        private AuthenticationDTO GenerateJwtToken(ApplicationUser user)
        {
            var jwtKey = _configuration["JwtTokenSettings:SecretKey"]!;
            var issuer = _configuration["JwtTokenSettings:Issuer"];
            var audience = _configuration["JwtTokenSettings:Audience"];
            var expireMinutes = int.Parse(_configuration["JwtTokenSettings:ExpirationInMinutes"]!);
            var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);
            var role = user.RoleId == Roles.AdminId ? Roles.Admin : Roles.Customer;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, role),
                new Claim("security_stamp", user.SecurityStamp ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthenticationDTO
            {
                AccessToken = tokenString,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserDTO>(user)
            };
        }
    }
}