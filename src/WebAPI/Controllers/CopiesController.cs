using Application.Dtos.Request;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
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
        [SwaggerOperation(Summary = "adds new copies")]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        
        public async Task<IActionResult> AddCopiesAsync([FromBody] CopyRequest dto)
        {
            await _service.AddAsync(dto);

            return Ok();
        }

        [HttpDelete(Copies.RemoveCopy)]
        [SwaggerOperation(Summary = "deletes excisting copy")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveCopyAsync([FromQuery] string inventory_number)
        {
            await _service.RemoveAsync(inventory_number);

            return Ok();
        }

        [HttpDelete(Copies.RemoveCopies)]
        [SwaggerOperation(Summary = "deletes excisting copies")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveCopiesAsync([FromBody] IEnumerable<string> inventory_numbers)
        {
            await _service.RemoveRangeAsync(inventory_numbers);

            return Ok();
        }
    }
}
