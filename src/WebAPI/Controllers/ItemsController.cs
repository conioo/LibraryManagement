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
using System.Globalization;
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

        [HttpGet(ApiRoutes.Items.GetAllItems)]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "returns all items")]
        [ProducesResponseType(typeof(IEnumerable<ItemResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllItemsAsync()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        [HttpGet(ApiRoutes.Items.GetPage)]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "returns the items page")]
        [ProducesResponseType(typeof(PagedResponse<ItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPageAsync([FromQuery] SieveModel sieveModel)
        {
            var pagedResponse = await _service.GetPageAsync(sieveModel);

            return Ok(pagedResponse);
        }

        [HttpGet(ApiRoutes.Items.GetItemById)]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "returns the item with the specified ID")]
        [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetItemByIdAsync([FromQuery] string id)
        {
            var item = await _service.GetByIdAsync(id);
            return Ok(item);
        }

        [HttpPost(ApiRoutes.Items.AddItem)]
        [SwaggerOperation(Summary = "adds new item")]
        [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddItemAsync([FromBody] ItemRequest dto)
        {
            var createdItem = await _service.AddAsync(dto);

            var uri = QueryHelpers.AddQueryString($"{_options.BaseAddress}/{_options.RoutePrefix}/{Items.Prefix}/{Items.GetItemById}", "id", createdItem.Id);
            return Created(uri, createdItem);
        }

        [HttpPost(ApiRoutes.Items.AddItems)]
        [SwaggerOperation(Summary = "adds new items")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddItemsAsync([FromBody] IEnumerable<ItemRequest> dtos)
        {
            await _service.AddRangeAsync(dtos);

            return Ok();
        }

        [HttpPut(ApiRoutes.Items.UpdateItem)]
        [SwaggerOperation(Summary = "updates excisting item")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateItemAsync([FromQuery] string id, [FromBody] ItemRequest dto)
        {
            await _service.UpdateAsync(id, dto);

            return Ok();
        }

        [HttpDelete(ApiRoutes.Items.RemoveItem)]
        [SwaggerOperation(Summary = "deletes excisting item")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveItemAsync([FromQuery] string id)
        {
            await _service.RemoveAsync(id);

            return Ok();
        }

        [HttpDelete(ApiRoutes.Items.RemoveItems)]
        [SwaggerOperation(Summary = "deletes excisting items")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveItemsAsync([FromBody] IEnumerable<string> ids)
        {
            await _service.RemoveRangeAsync(ids);

            return Ok();
        }
    }
}
