using Application.Dtos.Request;
using CommonContext;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using WebAPI.ApiRoutes;
using WebAPITests.Integration.SharedContextBuilders;

namespace WebAPITests.Integration
{
    public class CopiesControllerTests : IClassFixture<CopiesContextBuilder>, IDisposable
    {
        private readonly SharedContext _sharedContext;
        private readonly HttpClient _client;
        private readonly ApplicationUser _defaultUser;
        private readonly Library _library;
        private readonly Item _item;
        private readonly int _defaultNumberCopies;
        private readonly List<Copy> _copies;

        public CopiesControllerTests(CopiesContextBuilder sharedContextBuilder)
        {
            _sharedContext = sharedContextBuilder.Value;
            _client = _sharedContext.CreateClient();

            _defaultUser = DataGenerator.Get<ApplicationUser>(1).First();
            _defaultUser.UserName = "default";

            _sharedContext.UserManager.CreateAsync(_defaultUser, DataGenerator.GetUserPassword).Wait();

            _library = DataGenerator.Get<Library>(1).First();
            _item = DataGenerator.Get<Item>(1).First();

            _sharedContext.DbContext.Set<Library>().Add(_library);
            _sharedContext.DbContext.Set<Item>().Add(_item);

            _defaultNumberCopies = 2;

            _copies = DataGenerator.Get<Copy>(_defaultNumberCopies).ToList();

            _sharedContext.DbContext.Set<Copy>().AddRange(_copies);

            _sharedContext.DbContext.SaveChangesAsync().Wait();
        }

        public void Dispose()
        {
            _sharedContext.DbContext.Database.EnsureDeleted();
            _sharedContext.DbContext.Database.EnsureCreated();
        }

        [Fact]
        async Task AddCopiesAsync_ForValidModel_Returns200Ok()
        {
            var copyRequest = new CopyRequest()
            {
                ItemId = _item.Id,
                LibraryId = _library.Id,
                Count = 3
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Copies.AddCopies),
                Content = JsonContent.Create(copyRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies + 3);

            var newCopy = _sharedContext.DbContext.Set<Copy>().First(copy => copy.CreatedBy == "default");

            newCopy.CreatedBy.Should().Be("default");
            newCopy.LastModifiedBy.Should().Be("default");

            newCopy.Created.Should().NotBe(null);
            newCopy.LastModified.Should().NotBe(null);

            newCopy.Should().BeEquivalentTo(copyRequest, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task AddCopiesAsync_ForInvalidModel_Returns400BadRequest()
        {
            var copyRequest = new CopyRequest()
            {
                Count = 0,
                ItemId = null,
                LibraryId = "exist"
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Copies.AddCopies),
                Content = JsonContent.Create(copyRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(2);
            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies);
        }

        [Fact]
        async Task AddCopiesAsync_ForInvalidItemId_Returns404NotFound()
        {
            var copyRequest = new CopyRequest()
            {
                ItemId = "nullnull",
                LibraryId = _library.Id,
                Count = 3
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Copies.AddCopies),
                Content = JsonContent.Create(copyRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies);
        }

        [Fact]
        async Task AddCopiesAsync_ForInvalidLibraryId_Returns404NotFound()
        {
            var copyRequest = new CopyRequest()
            {
                ItemId = _item.Id,
                LibraryId = "nullnull",
                Count = 3
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Copies.AddCopies),
                Content = JsonContent.Create(copyRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies);
        }

        [Fact]
        async Task RemoveCopyAsync_ForValidId_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Copies.RemoveCopy + $"?inventory_number={_copies.First().InventoryNumber}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies - 1);
        }

        [Fact]
        async Task RemoveCopyAsync_ForInvalidId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Copies.RemoveCopy + $"?inventory_number=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies);
        }

        [Fact]
        async Task RemoveCopiesAsync_ForValidInventoryNumbers_Returns200Ok()
        {
            List<string> inventoryNumbers = new List<string>
            {
                _copies[0].InventoryNumber,
                _copies[1].InventoryNumber,
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Copies.RemoveCopies),
                Content = JsonContent.Create(inventoryNumbers)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(0);
        }

        [Fact]
        async Task RemoveCopiesAsync_OneInValidInventoryNumber_Returns404NotFound()
        {
            List<string> inventoryNumbers = new List<string>
            {
                _copies[0].InventoryNumber,
                "null_null",
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Copies.RemoveCopies),
                Content = JsonContent.Create(inventoryNumbers)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            _sharedContext.DbContext.Set<Copy>().Count().Should().Be(_defaultNumberCopies);
        }

    }
}
