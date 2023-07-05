using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces;
using Domain.Common;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.Worker}")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    public class ItemsController : ControllerBase
    {
        private readonly IItemService _service;
        private readonly ApplicationSettings _options;

        public ItemsController(IItemService service, IOptions<ApplicationSettings> options)
        {
            _service = service;
            _options = options.Value;
        }

        [HttpGet(Items.GetAllItems)]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "returns all items")]
        [ProducesResponseType(typeof(IEnumerable<ItemResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllItemsAsync()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        [HttpGet(Items.GetPage)]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "returns the items page")]
        [ProducesResponseType(typeof(PagedResponse<ItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPageAsync([FromQuery] SieveModel sieveModel)
        {
            var pagedResponse = await _service.GetPageAsync(sieveModel);

            return Ok(pagedResponse);
        }

        [HttpGet(Items.GetItemById)]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "returns the item with the specified ID")]
        [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetItemByIdAsync([FromQuery] string id)
        {
            var item = await _service.GetByIdAsync(id);
            return Ok(item);
        }

        [HttpGet(Items.GetAllCopies)]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "returns all copies")]
        [ProducesResponseType(typeof(IEnumerable<CopyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllCopiesAsync([FromQuery] string id)
        {
            var response = await _service.GetAllCopiesAsync(id);
            return Ok(response);
        }

        [HttpGet(Items.GetAvailableCopies)]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "returns available copies")]
        [ProducesResponseType(typeof(IEnumerable<CopyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailableCopiesAsync([FromQuery] string id)
        {
            var response = await _service.GetAvailableCopiesAsync(id);
            return Ok(response);
        }


        [HttpPost(Items.AddItem)]
        [SwaggerOperation(Summary = "adds new item")]
        [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddItemAsync([FromForm]ItemRequest dto)
        {
            var createdItem = await _service.AddAsync(dto);

            var uri = QueryHelpers.AddQueryString($"{_options.BaseAddress}/{_options.RoutePrefix}/{Items.Prefix}/{Items.GetItemById}", "id", createdItem.Id);
            return Created(uri, createdItem);
        }

        [HttpPost(Items.AddItems)]
        [SwaggerOperation(Summary = "adds new items")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddItemsAsync([FromForm]ICollection<ItemRequest> dtos)
        {
            await _service.AddRangeAsync(dtos);

            return Ok();
        }

        [HttpPut(Items.UpdateItem)]
        [SwaggerOperation(Summary = "updates excisting item")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateItemAsync([FromQuery] string id, [FromForm] UpdateItemRequest dto)
        {
            await _service.UpdateAsync(id, dto);

            return Ok();
        }

        [HttpDelete(Items.RemoveItem)]
        [SwaggerOperation(Summary = "deletes excisting item")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveItemAsync([FromQuery] string id)
        {
            await _service.RemoveAsync(id);

            return Ok();
        }

        [HttpDelete(Items.RemoveItems)]
        [SwaggerOperation(Summary = "deletes excisting items")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveItemsAsync([FromBody] IEnumerable<string> ids)
        {
            await _service.RemoveRangeAsync(ids);

            return Ok();
        }
    }
}