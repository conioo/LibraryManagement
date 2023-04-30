using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Dtos.Response.Archive;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileService _service;
        private readonly ApplicationSettings _applicationSettings;

        public ProfilesController(IProfileService service, IOptions<ApplicationSettings> options)
        {
            _service = service;
            _applicationSettings = options.Value;
        }

        [HttpPost(Profiles.CreateProfile)]
        [SwaggerOperation(Summary = "Creates new profile for user")]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateProfileAsync([FromBody] ProfileRequest dto)
        {
            var createdProfile = await _service.CreateProfileAsync(dto);

            var uri = QueryHelpers.AddQueryString($"{_applicationSettings.BaseAddress}/{_applicationSettings.RoutePrefix}/{Profiles.Prefix}/{Profiles.GetProfileByCardNumber}", "card-number", createdProfile.LibraryCardNumber);

            return Created(uri, createdProfile);
        }

        [HttpPatch(Profiles.ActivationProfile)]
        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Activates profile")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ActivationProfileAsync([FromBody] string cardNumber)
        {
            await _service.ActivationProfileAsync(cardNumber);

            return Ok();
        }

        [HttpPatch(Profiles.DeactivationProfile)]
        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Deactivates profile")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeactivationProfileAsync([FromBody] string cardNumber)
        {
            await _service.DeactivationProfileAsync(cardNumber);

            return Ok();
        }

        [HttpGet(Profiles.GetProfile)]
        [SwaggerOperation(Summary = "Returns own profile")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfileAsync()
        {
            var response = await _service.GetProfileAsync();

            return Ok(response);
        }

        [HttpGet(Profiles.GetProfileWithHistory)]
        [SwaggerOperation(Summary = "Returns own profile with history")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfileWithHistoryAsync()
        {
            var response = await _service.GetProfileWithHistoryAsync();

            return Ok(response);
        }

        [HttpGet(Profiles.GetProfileByCardNumber)]
        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Returns profile by card number")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfileByCardNumberAsync([FromQuery(Name = "card-number")] string cardNumber)
        {
            var response = await _service.GetProfileByCardNumberAsync(cardNumber);

            return Ok(response);
        }

        [HttpGet(Profiles.GetProfileWithHistoryByCardNumber)]
        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Returns profile with history")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfileWithHistoryByCardNumberAsync([FromQuery(Name = "card-number")] string cardNumber)
        {
            var response = await _service.GetProfileWithHistoryByCardNumberAsync(cardNumber);

            return Ok(response);
        }

        [HttpGet(Profiles.GetProfileHistoryByCardNumber)]
        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Returns profile with history")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProfileHistoryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfileHistoryByCardNumberAsync([FromQuery(Name = "card-number")] string cardNumber)
        {
            var response = await _service.GetProfileHistoryByCardNumberAsync(cardNumber);

            return Ok(response);
        }

        [HttpGet(Profiles.GetCurrentRentals)]
        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Returns current rentals")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(IEnumerable<RentalResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentRentalsAsync([FromQuery(Name = "card-number")] string cardNumber)
        {
            var response = await _service.GetCurrentRentalsAsync(cardNumber);

            return Ok(response);
        }

        [HttpGet(Profiles.GetCurrentReservations)]
        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Returns current reservations")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(IEnumerable<ReservationResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentReservationsAsync([FromQuery(Name = "card-number")] string cardNumber)
        {
            var response = await _service.GetCurrentReservationsAsync(cardNumber);

            return Ok(response);
        }
    }
}
