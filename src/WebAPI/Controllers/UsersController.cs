using Application.Dtos.Identity.Response;
using Application.Dtos.Response;
using Application.Interfaces;
using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.ApiRoutes;


namespace WebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}")]
        [HttpGet(Users.GetAllUsers)]
        [SwaggerOperation(Summary = "returns all users")]
        [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}")]
        [HttpGet(Users.GetPage)]
        [SwaggerOperation(Summary = "returns the users page")]
        [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPageAsync([FromQuery] SieveModel sieveModel)
        {
            var pagedResponse = await _service.GetPageAsync(sieveModel);

            return Ok(pagedResponse);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet(Users.GetUser)]
        [SwaggerOperation(Summary = "returns the user")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserAsync()
        {
            var user = await _service.GetUserAsync(User);

            return Ok(user);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}")]
        [HttpPut(Users.UpdateUser)]
        [SwaggerOperation(Summary = "update user")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserRequest dto)
        {
            await _service.UpdateAsync(User, dto);
            return Ok();
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}")]
        [HttpDelete(Users.RemoveUser)]
        [SwaggerOperation(Summary = "deletes excisting user")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveUserAsync([FromQuery] string id)
        {
            await _service.RemoveAsync(id);

            return Ok();
        }
    }
}
