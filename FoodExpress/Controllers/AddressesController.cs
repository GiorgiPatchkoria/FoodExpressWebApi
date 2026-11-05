using FoodExpress.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using FoodExpress.Responses;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Core.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FoodExpress.Controllers
{
    [ApiController, Authorize]
    [Route("api/addresses")]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _service;
        private readonly ICurrentUser _current;

        public AddressesController(IAddressService service, ICurrentUser current)
        {
            _service = service;
            _current = current;
        }

        /// <summary>
        /// Returns the current user's addresses.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<AddressDTO>>>> List(CancellationToken ct)
        {
            var result = await _service.ListAsync(_current.UserId, ct);

            var response = ApiResponse<IReadOnlyList<AddressDTO>>.Ok(result);

            return Ok(response);
        }

        /// <summary>
        /// Adds an address for the current user.
        /// </summary>
        /// <param name="request">The new address details.</param>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AddressDTO>>> Create(CreateAddressDTO request, CancellationToken ct)
        {
            return Created("/api/addresses", ApiResponse<AddressDTO>.Ok(await _service.CreateAsync(_current.UserId, request, ct)));
        }
    }
}