using Application.Dtos.Request;
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
using Reservations = WebAPI.ApiRoutes.Reservations;

namespace WebAPI.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}, {UserRoles.Basic}")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class ReservationsController : ControllerBase
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly IReservationService _service;

        public ReservationsController(IReservationService service, IOptions<ApplicationSettings> options)
        {
            _service = service;
            _applicationSettings = options.Value;
        }

        [HttpPost(Reservations.AddReservation)]
        [SwaggerOperation(Summary = "Create new reservation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddReservationAsync([FromBody] ReservationRequest request)
        {
            var newReservation = await _service.AddReservationAsync(request);

            var uri = QueryHelpers.AddQueryString($"{_applicationSettings.BaseAddress}/{_applicationSettings.RoutePrefix}/{Reservations.Prefix}/{Reservations.GetReservationById}", "id", newReservation.Id);

            return Created(uri, newReservation);
        }

        [HttpPost(Reservations.AddReservations)]
        [SwaggerOperation(Summary = "Create new Reservations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddReservationsAsync([FromBody] ICollection<ReservationRequest> requests)
        {
            await _service.AddReservationsAsync(requests);

            return Ok();
        }

        [HttpGet(Reservations.GetReservationById)]
        [SwaggerOperation(Summary = "Return Reservation by id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReservationByIdAsync([FromQuery] string id)
        {
            var Reservation = await _service.GetReservationById(id);

            return Ok(Reservation);
        }

        [HttpDelete(Reservations.RemoveReservationById)]
        [SwaggerOperation(Summary = "Remove Reservation by id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveReservationByIdAsync([FromQuery] string id)
        {
            await _service.RemoveReservationByIdAsync(id);

            return Ok();
        }

        [HttpPost(Reservations.Rent)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Rent item from reservation and returns a new rental")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RentAsync([FromQuery] string id)
        {
            var newRental = await _service.RentAsync(id);

            var uri = QueryHelpers.AddQueryString($"{_applicationSettings.BaseAddress}/{_applicationSettings.RoutePrefix}/{Rentals.Prefix}/{Rentals.GetRentalById}", "id", newRental.Id);

            return Created(uri, newRental);
        }
    }
}