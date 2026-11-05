using AutoMapper;
using Microsoft.AspNetCore.Identity;
using FoodExpress.Core.Models.Identity;
using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Services.Services.Interfaces;

namespace FoodExpress.Services.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRequestValidator _validator;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _current;

        public UserService(UserManager<ApplicationUser> userManager, IRequestValidator validator, IMapper mapper, ICurrentUser current)
        {
            _userManager = userManager;
            _validator = validator;
            _mapper = mapper;
            _current = current;
        }

        public async Task RegisterUserAsync(RegisterDTO dto, CancellationToken ct = default)
        {
            _validator.Validate(dto);
            var user = new ApplicationUser
            {
                Email = dto.Email.Trim().ToLowerInvariant(),
                UserName = dto.UserName.Trim().ToLowerInvariant(),
                FullName = dto.FullName.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                RoleId = Roles.CustomerId
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            CheckResult(result);
        }

        public async Task<UserDTO> GetAsync(int id, CancellationToken ct = default)
        {
            ServiceAccess.RequireOwner(_current, id);
            _validator.PositiveId(id);
            var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new ServiceException(ServiceError.NotFound, "User not found.");
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> UpdateAsync(int id, UpdateUserDTO dto, CancellationToken ct = default)
        {
            ServiceAccess.RequireOwner(_current, id);
            _validator.Validate(dto);
            var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new ServiceException(ServiceError.NotFound, "User not found.");
            var email = dto.Email.Trim().ToLowerInvariant();
            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(dto.CurrentPassword) || !await _userManager.CheckPasswordAsync(user, dto.CurrentPassword))
                {
                    throw new ServiceException(ServiceError.Validation, "Your current password is required to change your email address.");
                }
                user.EmailConfirmed = false;
            }
            user.FullName = dto.FullName.Trim();
            user.UserName = dto.UserName.Trim().ToLowerInvariant();
            user.Email = email;
            user.PhoneNumber = dto.PhoneNumber?.Trim();
            var result = await _userManager.UpdateAsync(user);
            CheckResult(result);
            return _mapper.Map<UserDTO>(user);
        }

        public async Task ChangePasswordAsync(int id, ChangePasswordDTO dto, CancellationToken ct = default)
        {
            ServiceAccess.RequireOwner(_current, id);
            _validator.Validate(dto);
            var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new ServiceException(ServiceError.NotFound, "User not found.");
            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            CheckResult(result);
        }

        private static void CheckResult(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                var duplicate = result.Errors.Any(error => error.Code.StartsWith("Duplicate", StringComparison.Ordinal));
                throw new ServiceException(duplicate ? ServiceError.Conflict : ServiceError.Validation, string.Join(", ", result.Errors.Select(error => error.Description)));
            }
        }
    }
}