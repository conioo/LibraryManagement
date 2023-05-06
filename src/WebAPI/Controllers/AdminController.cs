using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Interfaces;
using Domain.Settings;
using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.ApiRoutes;

namespace WebAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}")]
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _service;
        private readonly ApplicationSettings _options;

        public AdminController(IAdminService service, IOptions<ApplicationSettings> options)
        {
            _service = service;
            _options = options.Value;
        }

        [HttpPost(Admin.AddAdmin)]
        [SwaggerOperation(Summary = "add admin")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAdminAsync([FromBody] RegisterRequest dto)
        {
            var userResponse = await _service.AddAdmin(dto);

            var url = $"{_options.BaseAddress}/{_options.RoutePrefix}/{Users.Prefix}/{Users.GetUser}";

            return Created(url, userResponse);
        }

        [HttpPost(Admin.AddWorker)]
        [SwaggerOperation(Summary = "add worker")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddWorkerAsync([FromBody] RegisterRequest dto)
        {
            var userResponse = await _service.AddWorker(dto);

            var url = $"{_options.BaseAddress}/{_options.RoutePrefix}/{Users.Prefix}/{Users.GetUser}";

            return Created(url, userResponse);
        }
    }
}
