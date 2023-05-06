using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Dtos.Response.Archive;
using Application.Interfaces;
using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.ApiRoutes;

namespace WebAPI.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class CopiesController : ControllerBase
    {
        private readonly ICopyService _service;

        public CopiesController(ICopyService service)
        {
            _service = service;
        }


        [HttpPost(Copies.AddCopies)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Adds new copies")]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddCopiesAsync([FromBody] CopyRequest dto)
        {
            await _service.AddAsync(dto);

            return Ok();
        }

        [HttpGet(Copies.GetHistoryByInventoryNumber)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Returns copy history")]
        [ProducesResponseType(typeof(CopyHistoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetHistoryByInventoryNumberAsync([FromQuery(Name = "inventory-number")] string inventory_number)
        {
            var response = await _service.GetCopyHistoryAsync(inventory_number);

            return Ok(response);
        }

        [HttpGet(Copies.GetCopyById)]
        [SwaggerOperation(Summary = "Returns copy by inventory number")]
        [ProducesResponseType(typeof(CopyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCopyByIdAsync([FromQuery(Name = "inventory-number")] string inventory_number)
        {
            var response = await _service.GetByIdAsync(inventory_number);

            return Ok(response);
        }

        [HttpGet(Copies.GetCurrentRental)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Returns current rental")]
        [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCurrentRentalAsync([FromQuery(Name = "inventory-number")] string inventory_number)
        {
            var response = await _service.GetCurrentRentalAsync(inventory_number);

            return Ok(response);
        }

        [HttpGet(Copies.GetCurrentReservation)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Returns current reservation")]
        [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCurrentReservationAsync([FromQuery(Name = "inventory-number")] string inventory_number)
        {
            var response = await _service.GetCurrentReservationAsync(inventory_number);

            return Ok(response);
        }

        [HttpGet(Copies.IsAvailable)]
        [SwaggerOperation(Summary = "Checks whether copy is available")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> IsAvailableAsync([FromQuery(Name = "inventory-number")] string inventory_number)
        {
            var isAvailable = await _service.IsAvailableAsync(inventory_number);

            return Ok(isAvailable);
        }


        [HttpDelete(Copies.RemoveCopy)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Deletes excisting copy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveCopyAsync([FromQuery(Name = "inventory-number")] string inventory_number)
        {
            await _service.RemoveAsync(inventory_number);

            return Ok();
        }

        [HttpDelete(Copies.RemoveCopies)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
        [SwaggerOperation(Summary = "Deletes excisting copies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveCopiesAsync([FromBody] IEnumerable<string> inventory_numbers)
        {
            await _service.RemoveRangeAsync(inventory_numbers);

            return Ok();
        }
    }
}
