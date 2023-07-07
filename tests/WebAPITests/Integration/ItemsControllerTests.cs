using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces;
using CommonContext;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using Sieve.Models;
using System.Net.Http.Json;
using WebAPI.ApiRoutes;
using WebAPITests.Integration.SharedContextBuilders;

//mocka dodać

namespace WebAPITests.Integration
{
    public class ItemsControllerTests : IClassFixture<ItemContextBuilder>, IDisposable
    {
        private readonly HttpClient _client;
        private IEnumerable<Copy>? _copies;
        private readonly IEnumerable<Item> _items;
        private readonly SharedContext _sharedContext;

        public ItemsControllerTests(ItemContextBuilder sharedContextBuilder)
        {
            _sharedContext = sharedContextBuilder.Value;
            _client = _sharedContext.CreateClient();

            _items = DataGenerator.Get<Item>(3);

            _sharedContext.DbContext.Set<Item>().AddRange(_items);
            _sharedContext.DbContext.SaveChangesAsync().Wait();

            _sharedContext.RefreshDb();
        }

        [Fact]
        async Task AddItemAsync_ForInvalidModel_Returns400BadRequest()
        {
            var itemRequest = DataGenerator.GetOne<ItemRequest>();
            itemRequest.Title = "";

            var formDataContent = DataGenerator.GetMultipartFormDataContent(itemRequest);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Items.AddItem),
                Content = formDataContent,
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(1);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);
        }

        [Fact]
        async Task AddItemAsync_ForValidModelWithImage_Returns201Created()
        {
            var itemRequest = DataGenerator.GetOne<ItemRequest>();

            var formDataContent = DataGenerator.GetMultipartFormDataContent(itemRequest);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Items.AddItem),
                Content = formDataContent
            };

            var response = await _client.SendAsync(request);

            var responseItem = await response.Content.ReadFromJsonAsync<ItemResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Items.Prefix}/{Items.GetItemById}?id={responseItem.Id}");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().Be(expectedLocationUri);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(4);

            var newItem = await _sharedContext.DbContext.Set<Item>().FindAsync(responseItem.Id);

            newItem.CreatedBy.Should().Be("default");
            newItem.LastModifiedBy.Should().Be("default");

            newItem.Should().BeEquivalentTo(itemRequest, options => options.ExcludingMissingMembers());
            responseItem.Should().BeEquivalentTo(itemRequest, options => options.ExcludingMissingMembers());

            responseItem.ImagePaths.Count().Should().Be(1);
            responseItem.ImagePaths.First().Should().Be(@"C:\mock.png");
        }

        [Fact]
        async Task AddItemAsync_ForValidModelWithoutImage_Returns201Created()
        {
            var itemRequest = DataGenerator.GetOne<ItemRequest>();
            itemRequest.Images = null;

            var formDataContent = DataGenerator.GetMultipartFormDataContent(itemRequest);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Items.AddItem),
                Content = formDataContent
            };

            var response = await _client.SendAsync(request);

            var responseItem = await response.Content.ReadFromJsonAsync<ItemResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Items.Prefix}/{Items.GetItemById}?id={responseItem.Id}");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().Be(expectedLocationUri);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(4);

            var newItem = await _sharedContext.DbContext.Set<Item>().FindAsync(responseItem.Id);

            newItem.CreatedBy.Should().Be("default");
            newItem.LastModifiedBy.Should().Be("default");

            newItem.Should().BeEquivalentTo(itemRequest, options => options.ExcludingMissingMembers());
            responseItem.Should().BeEquivalentTo(itemRequest, options => options.ExcludingMissingMembers());

            responseItem.ImagePaths.Count().Should().Be(0);
        }

        [Fact]
        async Task AddItemsAsync_ForValidModels_Returns200Ok()
        {
            var itemsRequest = DataGenerator.Get<ItemRequest>(3);

            var formDataContent = DataGenerator.GetMultipartFormDataContentFromCollection((ICollection<ItemRequest>)itemsRequest, "dtos");

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Items.AddItems),
                Content = formDataContent
            };

            var ale = formDataContent.ToString();

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(6);

            var newItem = _sharedContext.DbContext.Set<Item>().Where(item => item.Title == itemsRequest.First().Title).First();
            newItem.ImagePaths.Count.Should().Be(1);
            newItem.ImagePaths.First().Should().Be("C:\\mock.png");
        }

        [Fact]
        async Task AddItemsAsync_OneInvalidModel_Returns400BadRequest()
        {
            var itemsRequest = DataGenerator.Get<ItemRequest>(3);

            itemsRequest.First().ISBN = "102";
            itemsRequest.First().Title = "";

            var formDataContent = DataGenerator.GetMultipartFormDataContentFromCollection((ICollection<ItemRequest>)itemsRequest, "dtos");

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Items.AddItems),
                Content = formDataContent
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count().Should().Be(2);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);
        }

        private async Task GenerateCopiesForFirstItem()
        {
            _copies = DataGenerator.GetWithDependencies<Copy>(3);

            _copies.ElementAt(2).IsAvailable = false;

            _items.First().Copies = (ICollection<Copy>)_copies;

            _sharedContext.DbContext.Set<Item>().Update(_items.First());
            await _sharedContext.DbContext.SaveChangesAsync();
        }

        [Fact]
        async Task GetAllCopiesAsync_ForEmptyCopies_Returns200Ok()
        {
            var requestUri = QueryHelpers.AddQueryString(Items.GetAllCopies, "id", _items.First().Id);

            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<CopyResponse>>();

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            result.Count().Should().Be(0);
        }

        [Fact]
        async Task GetAllCopiesAsync_ForInvalidId_Returns404NotFound()
        {
            var requestUri = QueryHelpers.AddQueryString(Items.GetAllCopies, "id", "null_null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetAllCopiesAsync_ForValidId_Returns200Ok()
        {
            await GenerateCopiesForFirstItem();

            var requestUri = QueryHelpers.AddQueryString(Items.GetAllCopies, "id", _items.First().Id);

            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<CopyResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            result.Should().BeEquivalentTo(_copies, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetAllItemsAsync_ForThreeItems_ReturnsAllItems()
        {
            var response = await _client.GetAsync(Items.GetAllItems);

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<ItemResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.Should().BeEquivalentTo(_items, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetAvailableCopiesAsync_ForEmptyCopies_Returns200Ok()
        {
            var requestUri = QueryHelpers.AddQueryString(Items.GetAvailableCopies, "id", _items.First().Id);

            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<CopyResponse>>();

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            result.Count().Should().Be(0);
        }

        [Fact]
        async Task GetAvailableCopiesAsync_ForInvalidId_Returns404NotFound()
        {
            var requestUri = QueryHelpers.AddQueryString(Items.GetAvailableCopies, "id", "null_null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetAvailableCopiesAsync_ForValidId_Returns200Ok()
        {
            await GenerateCopiesForFirstItem();

            var requestUri = QueryHelpers.AddQueryString(Items.GetAvailableCopies, "id", _items.First().Id);

            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<CopyResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            result.Should().BeEquivalentTo(_copies.Take(2), options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetItemByIdAsync_ForInvalidId_Returns404NotFound()
        {
            var requestUri = QueryHelpers.AddQueryString(Items.GetItemById, "id", "null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetItemByIdAsync_ForValidId_ReturnsCorrectItem()
        {
            var requestUri = QueryHelpers.AddQueryString(Items.GetItemById, "id", _items.First().Id);

            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<ItemResponse>();

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.Should().BeEquivalentTo(_items.First(), options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetPageAsync_ForInvalidSieveModel_Returns404BadRequest()
        {
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
        async Task GetPageAsync_ForNonExistingPage_ReturnsEmptyPage()
        {
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
            result.TotalPages.Should().Be(3);
            result.TotalItemsCount.Should().Be(3);
            result.Items.Should().BeEmpty();
        }

        [Fact]
        async Task GetPageAsync_ForValidPage_ReturnsCorrectPage()
        {
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
            result.TotalPages.Should().Be(2);
            result.TotalItemsCount.Should().Be(3);

            _items.Should().ContainEquivalentOf(result.Items.ElementAt(0));
        }

        [Fact]
        async Task GetPageAsync_UsingSortedPage_ReturnsCorrectSortedPage()
        {
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
        async Task RemoveItemAsync_ForInvalidId_Returns404NotFound()
        {
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
        async Task RemoveItemAsync_ForValidId_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Items.RemoveItem + $"?id={_items.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(2);
        }

        [Fact]
        async Task RemoveItemsAsync_ForValidIds_Returns200Ok()
        {
            List<string> ids = new List<string>
            {
                _items.ElementAt(2).Id,
                _items.ElementAt(1).Id,
                _items.ElementAt(0).Id
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
            List<string> ids = new List<string>
            {
                _items.ElementAt(2).Id,
                "null_null",
                _items.ElementAt(0).Id
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
        async Task UpdateItemAsync_ForInvalidModel_Returns400BadRequest()
        {
            var updateItem = new UpdateItemRequest()
            {
                FormOfPublication = Form.AudioBook,
                ISBN = "12345678910111213",
            };

            var formContent = DataGenerator.GetMultipartFormDataContent(updateItem);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Items.UpdateItem + $"?id={_items.First().Id}"),
                Content = formContent
            };

            var response = await _client.SendAsync(request);

            var dbItem = _sharedContext.DbContext.Set<Item>().Find(_items.First().Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(1);


            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);
            dbItem.Should().BeEquivalentTo(_items.First());

            dbItem.LastModified.Should().Be(dbItem.Created);
        }

        [Fact]
        async Task UpdateItemAsync_ForInvalidId_Returns404NotFound()
        {
            var updateItem = new UpdateItemRequest()
            {
                FormOfPublication = Form.AudioBook,
                ISBN = "1111111111111",
                Title = "updated title"
            };

            var formContent = DataGenerator.GetMultipartFormDataContent(updateItem);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Items.UpdateItem + $"?id=null_null"),
                Content = formContent
            };

            var response = await _client.SendAsync(request);

            var dbItem = _sharedContext.DbContext.Set<Item>().Find(_items.First().Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);

            dbItem.Should().BeEquivalentTo(_items.First());
            dbItem.LastModified.Should().Be(dbItem.Created);

            var filesServiceMock = _sharedContext.GetMock<IFilesService>();

            filesServiceMock.Verify(service => service.RemoveFiles(It.IsAny<ICollection<string>>()), Times.Never);
            filesServiceMock.Verify(service => service.SaveFilesAsync(It.IsAny<ICollection<IFormFile>>()), Times.Never);
        }

        [Fact]
        async Task UpdateItemAsync_ForValidModel_Returns200Ok()
        {
            var updateItem = new UpdateItemRequest()
            {
                FormOfPublication = Form.Film,
                ISBN = "1111111111111",
                Title = "updated title"
            };

            var formContent = DataGenerator.GetMultipartFormDataContent(updateItem);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Items.UpdateItem + $"?id={_items.First().Id}"),
                Content = formContent
            };

            var response = await _client.SendAsync(request);

            var dbItem = _sharedContext.DbContext.Set<Item>().Find(_items.First().Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);
            dbItem.LastModified.Should().BeAfter(dbItem.Created);
            dbItem.Title.Should().Be(updateItem.Title);
            dbItem.FormOfPublication.Should().Be(updateItem.FormOfPublication);
            dbItem.ISBN.Should().Be(updateItem.ISBN);
            dbItem.Should().BeEquivalentTo(_items.First(), opt => opt.Excluding(item => item.FormOfPublication).Excluding(item => item.Title).Excluding(item => item.ISBN).Excluding(item => item.LastModified).Excluding(item => item.LastModifiedBy));

            var filesServiceMock = _sharedContext.GetMock<IFilesService>();

            filesServiceMock.Verify(service => service.RemoveFiles(It.IsAny<ICollection<string>>()), Times.Never);
            filesServiceMock.Verify(service => service.SaveFilesAsync(It.IsAny<ICollection<IFormFile>>()), Times.Never);
        }

        [Fact]
        async Task UpdateItemAsync_ForValidModelWithCreateImage_Returns200Ok()
        {
            var updateItem = new UpdateItemRequest()
            {
                FormOfPublication = Form.Film,
                ISBN = "1111111111111",
                Title = "updated title",
                ImagesToCreate = new List<IFormFile>() { DataGenerator.GetImageFormFile("itemImage.png")}
            };

            var formContent = DataGenerator.GetMultipartFormDataContent(updateItem);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Items.UpdateItem + $"?id={_items.First().Id}"),
                Content = formContent
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);

            var dbItem = _sharedContext.DbContext.Set<Item>().Find(_items.First().Id);
            dbItem.LastModified.Should().BeAfter(dbItem.Created);
            dbItem.Title.Should().Be(updateItem.Title);
            dbItem.FormOfPublication.Should().Be(updateItem.FormOfPublication);
            dbItem.ISBN.Should().Be(updateItem.ISBN);
            dbItem.Should().BeEquivalentTo(_items.First(), opt => opt.Excluding(item => item.FormOfPublication).Excluding(item => item.Title).Excluding(item => item.ISBN).Excluding(item => item.LastModified).Excluding(item => item.LastModifiedBy).Excluding(item => item.ImagePaths));

            dbItem.ImagePaths.Count().Should().Be(2);
            dbItem.ImagePaths.Contains("C:\\mock.png").Should().BeTrue();
            dbItem.ImagePaths.Contains("fakeImage.png").Should().BeTrue();

            var filesServiceMock = _sharedContext.GetMock<IFilesService>();

            filesServiceMock.Verify(service => service.RemoveFiles(It.IsAny<ICollection<string>>()), Times.Never);
            filesServiceMock.Verify(service => service.SaveFilesAsync(It.IsAny<ICollection<IFormFile>>()), Times.Once);
        }

        [Fact]
        async Task UpdateItemAsync_ForValidModelWithDeleteImage_Returns200Ok()
        {
            var updateItem = new UpdateItemRequest()
            {
                FormOfPublication = Form.Film,
                ISBN = "1111111111111",
                Title = "updated title",
                ImagePathsToDelete = new List<string>() { "fakeImage.png" }
            };

            var formContent = DataGenerator.GetMultipartFormDataContent(updateItem);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Items.UpdateItem + $"?id={_items.First().Id}"),
                Content = formContent
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Item>().Count().Should().Be(3);

            var dbItem = _sharedContext.DbContext.Set<Item>().Find(_items.First().Id);
            dbItem.LastModified.Should().BeAfter(dbItem.Created);
            dbItem.Title.Should().Be(updateItem.Title);
            dbItem.FormOfPublication.Should().Be(updateItem.FormOfPublication);
            dbItem.ISBN.Should().Be(updateItem.ISBN);
            dbItem.Should().BeEquivalentTo(_items.First(), opt => opt.Excluding(item => item.FormOfPublication).Excluding(item => item.Title).Excluding(item => item.ISBN).Excluding(item => item.LastModified).Excluding(item => item.LastModifiedBy).Excluding(item => item.ImagePaths));

            dbItem.ImagePaths.Count().Should().Be(0);

            var filesServiceMock = _sharedContext.GetMock<IFilesService>();

            filesServiceMock.Verify(service => service.RemoveFiles(It.Is<ICollection<string>>(collection => collection.Count() == 1)), Times.Once);
            filesServiceMock.Verify(service => service.SaveFilesAsync(It.IsAny<ICollection<IFormFile>>()), Times.Never);
        }

        [Fact]
        async Task AddImagesAsync_ForValidModelWithImage_Returns200Ok()
        {
            var formFiles = new List<IFormFile>()
            {
                DataGenerator.GetImageFormFile("newImage1.png"),
                DataGenerator.GetImageFormFile("newImage2.png")
            };

            var formDataContent = DataGenerator.GetMultipartFormDataContentFromIFormFileCollection(formFiles, "images");

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Items.AddImages + $"?id={_items.First().Id}"),
                Content = formDataContent
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var updatedItem = await _sharedContext.DbContext.Set<Item>().FindAsync(_items.First().Id);

            updatedItem.LastModified.Should().BeAfter(updatedItem.Created);

            updatedItem.ImagePaths.Count().Should().Be(3);
            updatedItem.ImagePaths.Contains(@"C:\mock.png");
            updatedItem.ImagePaths.Contains("fakeImage.png");

            var filesServiceMock = _sharedContext.GetMock<IFilesService>();

            filesServiceMock.Verify(service => service.RemoveFiles(It.IsAny<ICollection<string>>()), Times.Never);
            filesServiceMock.Verify(service => service.SaveFilesAsync(It.Is<ICollection<IFormFile>>(collection => collection.Count() == 2)), Times.Once);
        }

        [Fact]
        async Task AddImagesAsync_ForInvalidId_Returns404NotFound()
        {
            var formFiles = new List<IFormFile>()
            {
                DataGenerator.GetImageFormFile("newImage1.png"),
            };

            var formDataContent = DataGenerator.GetMultipartFormDataContentFromIFormFileCollection(formFiles, "images");

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Items.AddImages + $"?id=null_null"),
                Content = formDataContent
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var updatedItem = await _sharedContext.DbContext.Set<Item>().FindAsync(_items.First().Id);

            updatedItem.LastModified.Should().Be(updatedItem.Created);

            updatedItem.ImagePaths.Count().Should().Be(1);
            updatedItem.ImagePaths.Contains("fakeImage.png");

            var filesServiceMock = _sharedContext.GetMock<IFilesService>();

            filesServiceMock.Verify(service => service.RemoveFiles(It.IsAny<ICollection<string>>()), Times.Never);
            filesServiceMock.Verify(service => service.SaveFilesAsync(It.IsAny<ICollection<IFormFile>>()), Times.Never);
        }

        [Fact]
        async Task AddImagesAsync_ForNoImages_Returns400BadRequest()
        {
            var formDataContent = new MultipartFormDataContent();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Items.AddImages + $"?id={_items.First().Id}"),
                Content = formDataContent
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count.Should().Be(1);

            var updatedItem = await _sharedContext.DbContext.Set<Item>().FindAsync(_items.First().Id);

            updatedItem.LastModified.Should().Be(updatedItem.Created);
            updatedItem.ImagePaths.Count().Should().Be(1);

            var filesServiceMock = _sharedContext.GetMock<IFilesService>();

            filesServiceMock.Verify(service => service.RemoveFiles(It.IsAny<ICollection<string>>()), Times.Never);
            filesServiceMock.Verify(service => service.SaveFilesAsync(It.IsAny<ICollection<IFormFile>>()), Times.Never);
        }

        [Fact]
        async Task RemoveImagesAsync_ForValidModelWithImage_Returns200Ok()
        {
            var filenames = new List<string>()
            {
                "fakeImage.png"
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Items.RemoveImages + $"?id={_items.First().Id}"),
                Content = JsonContent.Create(filenames)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var updatedItem = await _sharedContext.DbContext.Set<Item>().FindAsync(_items.First().Id);

            updatedItem.LastModified.Should().BeAfter(updatedItem.Created);

            updatedItem.ImagePaths.Count().Should().Be(0);

            var filesServiceMock = _sharedContext.GetMock<IFilesService>();

            filesServiceMock.Verify(service => service.RemoveFiles(It.Is<ICollection<string>>(collection => collection.Count() == 1)), Times.Once);
            filesServiceMock.Verify(service => service.SaveFilesAsync(It.IsAny<ICollection<IFormFile>>()), Times.Never);
        }

        [Fact]
        async Task RemoveImagesAsync_ForInvalidId_Returns404NotFound()
        {
            var filenames = new List<string>()
            {
                "fakeImage.png"
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Items.RemoveImages + $"?id=null_null"),
                Content = JsonContent.Create(filenames)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var updatedItem = await _sharedContext.DbContext.Set<Item>().FindAsync(_items.First().Id);

            updatedItem.LastModified.Should().Be(updatedItem.Created);

            updatedItem.ImagePaths.Count().Should().Be(1);

            var filesServiceMock = _sharedContext.GetMock<IFilesService>();

            filesServiceMock.Verify(service => service.RemoveFiles(It.IsAny<ICollection<string>>()), Times.Never);
            filesServiceMock.Verify(service => service.SaveFilesAsync(It.IsAny<ICollection<IFormFile>>()), Times.Never);
        }

        [Fact]
        async Task RemoveImagesAsync_ForNoFilenames_Returns400BadRequest()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Items.RemoveImages + $"?id={_items.First().Id}"),
                Content = JsonContent.Create("")
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var updatedItem = await _sharedContext.DbContext.Set<Item>().FindAsync(_items.First().Id);

            updatedItem.LastModified.Should().Be(updatedItem.Created);

            updatedItem.ImagePaths.Count().Should().Be(1);

            var filesServiceMock = _sharedContext.GetMock<IFilesService>();

            filesServiceMock.Verify(service => service.RemoveFiles(It.IsAny<ICollection<string>>()), Times.Never);
            filesServiceMock.Verify(service => service.SaveFilesAsync(It.IsAny<ICollection<IFormFile>>()), Times.Never);
        }

        [Fact]
        async Task RemoveImagesAsync_ForOneInvalidFilename_Returns400BadRequest()
        {
            var filenames = new List<string>()
            {
                "fakeImage.png",
                "invalid.png"
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Items.RemoveImages + $"?id={_items.First().Id}"),
                Content = JsonContent.Create(filenames)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var updatedItem = await _sharedContext.DbContext.Set<Item>().FindAsync(_items.First().Id);

            updatedItem.LastModified.Should().Be(updatedItem.Created);

            updatedItem.ImagePaths.Count().Should().Be(1);

            var filesServiceMock = _sharedContext.GetMock<IFilesService>();

            filesServiceMock.Verify(service => service.RemoveFiles(It.IsAny<ICollection<string>>()), Times.Never);
            filesServiceMock.Verify(service => service.SaveFilesAsync(It.IsAny<ICollection<IFormFile>>()), Times.Never);
        }

        public void Dispose()
        {
            _sharedContext.ResetState();
        }
    }
}