using Application.Dtos.Request;
using Application.Dtos.Response;
using CommonContext;
using CommonContext.SharedContextBuilders;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Sieve.Models;
using System.Net.Http.Json;
using WebAPI.ApiRoutes;

namespace WebAPITests.Integration
{
    public class ItemsControllerTests : IClassFixture<ItemContextBuilder>, IDisposable
    {
        private readonly SharedContext _sharedContext;
        private readonly HttpClient _client;

        public ItemsControllerTests(ItemContextBuilder sharedContextBuilder)
        {
            _sharedContext = sharedContextBuilder.Value;
            _client = _sharedContext.CreateClient();
        }

        public void Dispose()
        {
            _sharedContext.DbContext.Database.EnsureDeleted();
            _sharedContext.DbContext.Database.EnsureCreated();
        }

        [Fact]
        async Task GetAllItemsAsync_ForThreeItems_ReturnsAllItems()
        {
            var items = DataGenerator.Get<Item>(3);

            _sharedContext.DbContext.Set<Item>().AddRange(items);
            await _sharedContext.DbContext.SaveChangesAsync();

            _sharedContext.RefreshDb();

            var response = await _client.GetAsync(Items.GetAllItems);
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<ItemResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.Should().BeEquivalentTo(items, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetItemByIdAsync_ForValidId_ReturnsCorrectItem()
        {
            var items = DataGenerator.Get<Item>(3);

            _sharedContext.DbContext.Set<Item>().AddRange(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            var requestUri = QueryHelpers.AddQueryString(Items.GetItemById, "id", items.First().Id);

            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<ItemResponse>();

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.Should().BeEquivalentTo(items.First(), options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetItemByIdAsync_ForInvalidId_Returns404NotFound()
        {
            var items = DataGenerator.Get<Item>(3);

            _sharedContext.DbContext.Set<Item>().AddRange(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            var requestUri = QueryHelpers.AddQueryString(Items.GetItemById, "id", "null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetPageAsync_ForValidPage_ReturnsCorrectPage()
        {
            var items = DataGenerator.Get<Item>(5);

            _sharedContext.DbContext.Set<Item>().AddRange(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            var sieveModel = new SieveModel()
            {
                PageSize = 2,
                Page = 2,
            };

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Items.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<ItemResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.TotalPages.Should().Be(3);
            result.TotalItemsCount.Should().Be(5);

            items.Should().ContainEquivalentOf(result.Items.ElementAt(0));
            items.Should().ContainEquivalentOf(result.Items.ElementAt(1));
        }

        [Fact]
        async Task GetPageAsync_ForNonExistingPage_ReturnsEmptyPage()
        {
            var items = DataGenerator.Get<Item>(5);

            _sharedContext.DbContext.Set<Item>().AddRange(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            var sieveModel = new SieveModel()
            {
                PageSize = 1,
                Page = 10,
            };

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Items.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<ItemResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.TotalPages.Should().Be(5);
            result.TotalItemsCount.Should().Be(5);
            result.Items.Should().BeEmpty();
        }

        [Fact]
        async Task GetPageAsync_ForInvalidSieveModel_Returns404BadRequest()
        {
            var items = DataGenerator.Get<Item>(5);

            _sharedContext.DbContext.Set<Item>().AddRange(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            var sieveModel = new SieveModel();

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Items.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count().Should().Be(2);
        }

        [Fact]
        async Task GetPageAsync_UsingSortedPage_ReturnsCorrectSortedPage()
        {
            var items = DataGenerator.Get<Item>(5);

            _sharedContext.DbContext.Set<Item>().AddRange(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            var sieveModel = new SieveModel()
            {
                Page = 1,
                PageSize = 3,
                Sorts = "year"
            };

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Items.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<ItemResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            result.Items.Count().Should().Be(3);

            result.Items.Should().BeInAscendingOrder(item => item.YearOfPublication);
        }

        [Fact]
        async Task AddItemAsync_ForValidModel_Returns201Created()
        {
            var itemRequest = DataGenerator.GetRequest<ItemRequest>(1).First();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Items.AddItem),
                Content = JsonContent.Create(itemRequest)
            };

            var response = await _client.SendAsync(request);

            var responseItem = await response.Content.ReadFromJsonAsync<ItemResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Items.Prefix}/{Items.GetItemById}?id={responseItem.Id}");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().Be(expectedLocationUri);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(1);

            _sharedContext.DbContext.Set<Item>().First().CreatedBy.Should().Be("default");
            _sharedContext.DbContext.Set<Item>().First().LastModifiedBy.Should().Be("default");

            _sharedContext.DbContext.Set<Item>().First().Should().BeEquivalentTo(itemRequest, options => options.ExcludingMissingMembers());
            responseItem.Should().BeEquivalentTo(itemRequest, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task AddItemAsync_ForInvalidModel_Returns400BadRequest()
        {
            var itemRequest = DataGenerator.GetRequest<ItemRequest>(1).First();

            itemRequest.Title = "";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Items.AddItem),
                Content = JsonContent.Create(itemRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(1);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(0);
        }

        [Fact]
        async Task AddItemsAsync_ForValidModels_Returns200Ok()
        {
            var itemsRequest = DataGenerator.GetRequest<ItemRequest>(3);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Items.AddItems),
                Content = JsonContent.Create(itemsRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);

            _sharedContext.DbContext.Set<Item>().First().CreatedBy.Should().Be("default");
            _sharedContext.DbContext.Set<Item>().First().LastModifiedBy.Should().Be("default");
        }

        [Fact]
        async Task AddItemsAsync_OneInvalidModel_Returns400BadRequest()
        {
            var itemsRequest = DataGenerator.GetRequest<ItemRequest>(3);

            itemsRequest.First().ISBN = "102";
            itemsRequest.First().Title = "";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Items.AddItems),
                Content = JsonContent.Create(itemsRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count().Should().Be(2);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(0);
        }

        [Fact]
        async Task RemoveItemAsync_ForValidId_Returns200Ok()
        {
            var items = DataGenerator.Get<Item>(3);

            _sharedContext.DbContext.Set<Item>().AddRange(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Items.RemoveItem + $"?id={items.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(2);
        }

        [Fact]
        async Task RemoveItemAsync_ForInvalidId_Returns404NotFound()
        {
            var items = DataGenerator.Get<Item>(3);

            _sharedContext.DbContext.Set<Item>().AddRange(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Items.RemoveItem + $"?id=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);
        }

        [Fact]
        async Task RemoveItemsAsync_ForValidIds_Returns200Ok()
        {
            var items = DataGenerator.Get<Item>(3);

            _sharedContext.DbContext.Set<Item>().AddRange(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            List<string> ids = new List<string>
            {
                items.ElementAt(2).Id,
                items.ElementAt(1).Id,
                items.ElementAt(0).Id
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Items.RemoveItems),
                Content = JsonContent.Create(ids)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(0);
        }

        [Fact]
        async Task RemoveItemsAsync_OneInValidId_Returns404NotFound()
        {
            var items = DataGenerator.Get<Item>(3);

            _sharedContext.DbContext.Set<Item>().AddRange(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            List<string> ids = new List<string>
            {
                items.ElementAt(2).Id,
                "null_null",
                items.ElementAt(0).Id
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Items.RemoveItems),
                Content = JsonContent.Create(ids)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);
        }

        [Fact]
        async Task UpdateItemAsync_ForValidModel_Returns200Ok()
        {
            var items = DataGenerator.Get<Item>(3);

            await _sharedContext.DbContext.Set<Item>().AddRangeAsync(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            var updateItem = DataGenerator._mapper.Map<ItemRequest>(items.First());

            updateItem.FormOfPublication = Form.Film;
            updateItem.ISBN = "1111111111111";
            updateItem.Title = "updated title";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Items.UpdateItem + $"?id={items.First().Id}"),
                Content = JsonContent.Create(updateItem)
            };

            var response = await _client.SendAsync(request);

            var dbItem = _sharedContext.DbContext.Set<Item>().Find(items.First().Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);
            dbItem.Should().BeEquivalentTo(updateItem, options => options.ExcludingMissingMembers());
            dbItem.LastModified.Should().BeAfter(dbItem.Created);
        }

        [Fact]
        async Task UpdateItemAsync_ForInvalidModel_Returns400BadRequest()
        {
            var items = DataGenerator.Get<Item>(3);

            await _sharedContext.DbContext.Set<Item>().AddRangeAsync(items);
            await _sharedContext.DbContext.SaveChangesAsync();
            _sharedContext.RefreshDb();

            var updateItem = DataGenerator._mapper.Map<ItemRequest>(items.First());

            updateItem.FormOfPublication = Form.AudioBook;
            updateItem.ISBN = "12345678910111213";
            updateItem.Title = "";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Items.UpdateItem + $"?id=null_null"),
                Content = JsonContent.Create(updateItem)
            };

            var response = await _client.SendAsync(request);

            var dbItem = _sharedContext.DbContext.Set<Item>().Find(items.First().Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(2);


            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);
            dbItem.Should().NotBeEquivalentTo(updateItem, options => options.ExcludingMissingMembers());

            dbItem.LastModified.Should().Be(dbItem.Created);
        }

    }
}
