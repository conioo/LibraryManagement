using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Interfaces;
using Domain.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.ApiRoutes;

namespace WebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _service;
        private readonly ApplicationSettings _options;

        public AccountsController(IAccountService service, IOptions<ApplicationSettings> options)
        {
            _service = service;
            _options = options.Value;
        }

        [HttpPost(Accounts.Register)]
        [SwaggerOperation(Summary = "register user")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest dto)
        {
            var newUser = await _service.RegisterAsync(dto);

            var uri = $"{_options.BaseAddress}/{_options.RoutePrefix}/{Users.Prefix}/{Users.GetUser}";

            return Created(uri, newUser);
        }

        [HttpGet(Accounts.ConfirmEmail)]
        [SwaggerOperation(Summary = "confirm email address")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userid, [FromQuery] string token)
        {
            await _service.ConfirmEmailAsync(userid, token);
            return Ok();
        }

        [SwaggerOperation(Summary = "reset user password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [HttpPost(Accounts.ResetPassword)]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest dto)
        {
            await _service.ResetPasswordAsync(dto);
            return Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [SwaggerOperation(Summary = "logout user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpHead(Accounts.Logout)]
        public async Task<IActionResult> LogoutAsync()
        {
            await _service.Logout(User);
            return Ok();
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [SwaggerOperation(Summary = "change user password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost(Accounts.ChangePassword)]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest dto)
        {
            await _service.ChangePasswordAsync(dto, User);
            return Ok();
        }

        [SwaggerOperation(Summary = "forgot user password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [HttpPost(Accounts.ForgotPassword)]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest dto)
        {
            await _service.ForgotPasswordAsync(dto);
            return Ok();
        }

        [SwaggerOperation(Summary = "user login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [HttpPost(Accounts.Login)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest dto)
        {
            var response = await _service.LoginAsync(dto);
            return Ok(response);
        }

        [SwaggerOperation(Summary = "returns new jwt")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet(Accounts.RefreshToken)]
        public async Task<IActionResult> RefreshTokenAsync([FromQuery()] string refresh_token)
        {
            var token = await _service.RefreshToken(refresh_token);
            return Ok(token);
        }
    }
}