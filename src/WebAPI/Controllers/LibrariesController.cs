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
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.ApiRoutes;

namespace WebAPI.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class LibrariesController : ControllerBase
    {
        private readonly ILibraryService _service;
        private readonly ApplicationSettings _options;

        public LibrariesController(ILibraryService service, IOptions<ApplicationSettings> options)
        {
            _service = service;
            _options = options.Value;
        }

        [HttpGet(Libraries.GetAllLibraries)]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "returns all libraries")]
        [ProducesResponseType(typeof(IEnumerable<LibraryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            var Librarys = await _service.GetAllAsync();
            return Ok(Librarys);
        }

        [HttpGet(Libraries.GetPage)]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "returns the Libraries page")]
        [ProducesResponseType(typeof(PagedResponse<LibraryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPageAsync([FromQuery] SieveModel sieveModel)
        {
            var pagedResponse = await _service.GetPageAsync(sieveModel);

            return Ok(pagedResponse);
        }

        [HttpGet(Libraries.GetLibraryById)]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "returns the Library with the specified ID")]
        [ProducesResponseType(typeof(LibraryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLibraryByIdAsync([FromQuery] string id)
        {
            var Library = await _service.GetByIdAsync(id);
            return Ok(Library);
        }

        [HttpPost(Libraries.AddLibrary)]
        [SwaggerOperation(Summary = "adds new Library")]
        [ProducesResponseType(typeof(LibraryResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddLibraryAsync([FromBody] LibraryRequest dto)
        {
            var createdLibrary = await _service.AddAsync(dto);

            var uri = QueryHelpers.AddQueryString($"{_options.BaseAddress}/{_options.RoutePrefix}/{Libraries.Prefix}/{Libraries.GetLibraryById}", "id", createdLibrary.Id);
            return Created(uri, createdLibrary);
        }

        [HttpPut(Libraries.UpdateLibrary)]
        [SwaggerOperation(Summary = "updates excisting Library")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateLibraryAsync([FromQuery] string id, [FromBody] LibraryRequest dto)
        {
            await _service.UpdateAsync(id, dto);

            return Ok();
        }

        [HttpDelete(Libraries.RemoveLibrary)]
        [SwaggerOperation(Summary = "deletes excisting Library")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveLibraryAsync([FromQuery] string id)
        {
            await _service.RemoveAsync(id);

            return Ok();
        }
    }
}
