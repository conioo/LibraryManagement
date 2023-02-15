using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Dtos.Response;
using Application.Interfaces;
using Domain.Settings;
using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.ApiRoutes;

namespace WebAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}")]
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class RolesController : ControllerBase
    {
        private IRoleService _service;
        private readonly ApplicationSettings _options;

        public RolesController(IRoleService service, IOptions<ApplicationSettings> options)
        {
            _service = service;
            _options = options.Value;
        }

        [HttpGet(Roles.GetAllRoles)]
        [SwaggerOperation(Summary = "returns all roles")]
        [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            var response = await _service.GetAllAsync();

            return Ok(response);
        }

        [HttpGet(Roles.GetPage)]
        [SwaggerOperation(Summary = "returns page role")]
        [ProducesResponseType(typeof(PagedResponse<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPageAsync([FromQuery] SieveModel sieveModel)
        {
            var pageResponse = await _service.GetPageAsync(sieveModel);

            return Ok(pageResponse);
        }

        [HttpGet(Roles.GetRoleById)]
        [SwaggerOperation(Summary = "returns role by id")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleByIdAsync([FromQuery] string id)
        {
            var response = await _service.GetByIdAsync(id);

            return Ok(response);
        }

        [HttpPost(Roles.GetUsersInRole)]
        [SwaggerOperation(Summary = "returns users in role by role id")]
        [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUsersInRoleAsync([FromQuery] string roleId)
        {
            var response = await _service.GetUsersInRoleAsync(roleId);

            return Ok(response);
        }

        [HttpGet(Roles.GetRolesByUser)]
        [SwaggerOperation(Summary = "returns roles by user id")]
        [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRolesByUserAsync([FromQuery] string userId)
        {
            var response = await _service.GetRolesByUserAsync(userId);

            return Ok(response);
        }

        [HttpPost(Roles.AddRole)]
        [SwaggerOperation(Summary = "create new role")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddRoleAsync([FromBody] RoleRequest dto)
        {
            var response = await _service.AddAsync(dto);

            var uri = $"{_options.BaseAddress}/{_options.RoutePrefix}/{Roles.Prefix}/{Roles.GetRoleById}?id={response.Id}";

            return Created(uri, response);
        }

        [HttpPost(Roles.AddUsersToRole)]
        [SwaggerOperation(Summary = "add users to role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddUsersToRoleAsync([FromBody] RoleModificationRequest dto)
        {
            await _service.AddUsersToRoleAsync(dto);

            return Ok();
        }

        [HttpPost(Roles.RemoveRoleFromUsers)]
        [SwaggerOperation(Summary = "remove role from users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveRoleFromUsersAsync([FromBody] RoleModificationRequest dto)
        {
            await _service.RemoveRoleFromUsersAsync(dto);

            return Ok();
        }

        [HttpPut(Roles.UpdateRole)]
        [SwaggerOperation(Summary = "update role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRoleAsync([FromBody] RoleRequest dto, [FromQuery] string id)
        {
            await _service.UpdateAsync(id, dto);

            return Ok();
        }

        [HttpDelete(Roles.RemoveRole)]
        [SwaggerOperation(Summary = "remove role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRoleAsync([FromQuery] string id)
        {
            await _service.RemoveAsync(id);

            return Ok();
        }
    }
}