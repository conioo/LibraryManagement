using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces;
using Domain.Settings;
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
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateProfileAsync([FromBody] ProfileRequest dto)
        {
            var createdProfile = await _service.CreateProfileAsync(dto);

            var uri = QueryHelpers.AddQueryString($"{_applicationSettings.BaseAddress}/{_applicationSettings.RoutePrefix}/{Profiles.Prefix}/{Profiles.GetProfileByCardNumber}", "card-number", createdProfile.LibraryCardNumber);

            return Created(uri, createdProfile);
        }

        [HttpPost(Profiles.ActivationProfile)]
        [SwaggerOperation(Summary = "Activates profile")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ActivationProfileAsync([FromBody] string cardNumber)
        {
            await _service.ActivationProfileAsync(cardNumber);

            return Ok();
        }

        [HttpGet(Profiles.GetProfileByCardNumber)]
        [SwaggerOperation(Summary = "Returns profile")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfileByCardNumberAsync([FromQuery(Name = "card-number")] string cardNumber)
        {
            var response = await _service.GetProfileByCardNumberAsync(cardNumber);

            return Ok(response);
        }

        [HttpGet(Profiles.GetProfileByCardNumber+"his")]
        [SwaggerOperation(Summary = "Returns profile with history")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfileWithHistoryByCardNumberAsync([FromQuery(Name = "card-number")] string cardNumber)
        {
            var response = await _service.GetProfileWithHistoryByCardNumberAsync(cardNumber);

            return Ok(response);
        }

        [HttpGet(Profiles.GetProfileByCardNumber+"un")]
        [SwaggerOperation(Summary = "Returns unreturned history rentals")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(IEnumerable<RentalResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreturnedRentalsAsync([FromQuery(Name = "card-number")] string cardNumber)
        {
            var response = await _service.GetUnreturnedRentalsAsync(cardNumber);

            return Ok(response);
        }

        [HttpGet(Profiles.GetProfileByCardNumber+"rec")]
        [SwaggerOperation(Summary = "Returns unreceived history reservations")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(IEnumerable<ReservationResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreceivedReservationsAsync([FromQuery(Name = "card-number")] string cardNumber)
        {
            var response = await _service.GetUnreceivedReservationsAsync(cardNumber);

            return Ok(response);
        }

        [HttpGet(Profiles.GetRentalHistory)]
        [SwaggerOperation(Summary = "Returns rental history")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(IEnumerable<RentalResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRentalHistoryAsync([FromQuery(Name = "card-number")] string cardNumber)
        {
            var response = await _service.GetRentalHistoryAsync(cardNumber);

            return Ok(response);
        }

        [HttpGet(Profiles.GetReservationHistory)]
        [SwaggerOperation(Summary = "Returns rental history")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(IEnumerable<ReservationResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReservationHistoryAsync([FromQuery(Name = "card-number")] string cardNumber)
        {
            var response = await _service.GetReservationHistoryAsync(cardNumber);

            return Ok(response);
        }
    }
}
