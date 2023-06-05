using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces;
using Domain.Settings;
using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.ApiRoutes;

namespace WebAPI.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService _service;
        private readonly ApplicationSettings _applicationSettings;

        public RentalsController(IRentalService service, IOptions<ApplicationSettings> options)
        {
            _service = service;
            _applicationSettings = options.Value;
        }

        [HttpGet(Rentals.GetRentalById)]
        [SwaggerOperation(Summary = "Return rental by id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRentalByIdAsync([FromQuery] string id)
        {
            var rental = await _service.GetRentalById(id);

            return Ok(rental);
        }

        [HttpPost(Rentals.AddRental)]
        [SwaggerOperation(Summary = "Create new rental")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddRentalAsync([FromBody] RentalRequest request, [FromQuery] string profileLibraryCardNumber)
        {
            var newRental = await _service.AddRentalAsync(request, profileLibraryCardNumber);

            var uri = QueryHelpers.AddQueryString($"{_applicationSettings.BaseAddress}/{_applicationSettings.RoutePrefix}/{Rentals.Prefix}/{Rentals.GetRentalById}", "id", newRental.Id);

            return Created(uri, newRental);
        }

        [HttpPost(Rentals.AddRentals)]
        [SwaggerOperation(Summary = "Create new rentals")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddRentalsAsync([FromBody] ICollection<RentalRequest> requests, [FromQuery] string profileLibraryCardNumber)
        {
            await _service.AddRentalsAsync(requests, profileLibraryCardNumber);

            return Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}")]
        [HttpDelete(Rentals.RemoveRentalById)]
        [SwaggerOperation(Summary = "Remove rental by id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRentalByIdAsync([FromQuery] string id)
        {
            await _service.RemoveRentalByIdAsync(id);

            return Ok();
        }

        [HttpPatch(Rentals.Renewal)]
        [SwaggerOperation(Summary = "Renewal rental by id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RenewalAsync([FromQuery] string id)
        {
            await _service.RenewalAsync(id);

            return Ok();
        }

        [HttpPatch(Rentals.Return)]
        [SwaggerOperation(Summary = "Return copy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReturnAsync([FromQuery] string id)
        {
            await _service.ReturnAsync(id);

            return Ok();
        }

        [HttpPatch(Rentals.Returns)]
        [SwaggerOperation(Summary = "Return coppies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReturnsAsync([FromBody] IEnumerable<string> ids)
        {
            await _service.ReturnsAsync(ids);

            return Ok();
        }
    }
}