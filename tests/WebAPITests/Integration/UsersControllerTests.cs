using Application.Dtos.Identity.Response;
using Application.Dtos.Response;
using CommonContext;
using FluentAssertions;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Sieve.Models;
using System.Net;
using System.Net.Http.Json;
using WebAPI.ApiRoutes;
using WebAPITests.Integration.SharedContextBuilders;

namespace WebAPITests.Integration
{
    public class UsersControllerTests : IClassFixture<UserContextBuilder>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly ApplicationUser _defaultUser;
        private readonly SharedContext _sharedContext;

        private readonly List<ApplicationUser> _users;

        public UsersControllerTests(UserContextBuilder userContextBuilder)
        {
            _sharedContext = userContextBuilder.Value;
            _client = _sharedContext.CreateClient();

            var admin = _sharedContext.IdentityDbContext.Users.Single(user => user.UserName == "Admin");

            var users = DataGenerator.Get<ApplicationUser>(2);

            _defaultUser = _sharedContext.DefaultUser;

            _sharedContext.UserManager.CreateAsync(users.ElementAt(0), DataGenerator.GetUserPassword).Wait();
            _sharedContext.UserManager.CreateAsync(users.ElementAt(1), DataGenerator.GetUserPassword).Wait();

            _users = (List<ApplicationUser>)users;
            _users.Add(admin);
            _users.Add(_defaultUser);
        }

        [Fact]
        async Task GetAllUsersAsync_ForFourUsers_ReturnsAllUsers()
        {
            var response = await _client.GetAsync(Users.GetAllUsers);
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            result.Count().Should().Be(4);
            result.Should().BeEquivalentTo(_users, options => options.ExcludingMissingMembers());
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

            var requestUri = QueryHelpers.AddQueryString(Users.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

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

            var requestUri = QueryHelpers.AddQueryString(Users.GetPage, queryString);

            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserResponse>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.TotalPages.Should().Be(4);
            result.TotalItemsCount.Should().Be(4);
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

            var requestUri = QueryHelpers.AddQueryString(Users.GetPage, queryString);

            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserResponse>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.TotalPages.Should().Be(2);
            result.TotalItemsCount.Should().Be(4);

            _users.Should().ContainEquivalentOf(result.Items.ElementAt(0));
            _users.Should().ContainEquivalentOf(result.Items.ElementAt(1));
        }

        [Fact]
        async Task GetPageAsync_UsingSortedPage_ReturnsCorrectSortedPage()
        {
            var sieveModel = new SieveModel()
            {
                Page = 1,
                PageSize = 3,
                Sorts = "FirstName"
            };

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Items.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserResponse>>();

            result.Items.Count().Should().Be(3);

            result.Items.Should().BeInAscendingOrder(user => user.FirstName);
        }

        [Fact]
        async Task GetUserAsync_ForValidJwt_ReturnsCorrectUser()
        {
            var response = await _client.GetAsync(_client.BaseAddress + Users.GetUser);

            var userResponse = await response.Content.ReadFromJsonAsync<UserResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            userResponse.Should().BeEquivalentTo(_defaultUser, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task RemoveUserAsync_ForInvalidId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Items.RemoveItem + $"?id=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _sharedContext.IdentityDbContext.Users.Count().Should().Be(4);
        }

        [Fact]
        async Task RemoveUserAsync_ForValidId_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Users.RemoveUser + $"?id={_users.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _sharedContext.IdentityDbContext.Users.Count().Should().Be(3);
            _sharedContext.IdentityDbContext.Users.Should().NotContain(user => user.Id == _users.First().Id);
        }

        [Fact]
        async Task UpdateItemAsync_ForInvalidModel_Returns400BadRequest()
        {
            var updateUser = new UpdateUserRequest();

            updateUser.UserName = "al";
            updateUser.FirstName = "";
            updateUser.LastName = "";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Items.UpdateItem),
                Content = JsonContent.Create(updateUser)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(3);

            _sharedContext.IdentityDbContext.Users.Find(_defaultUser.Id).Should().NotBe("al");
        }

        [Fact]
        async Task UpdateItemAsync_ForValidModel_Returns200Ok()
        {
            var updateUser = new UpdateUserRequest();

            updateUser.UserName = "dddssa";
            updateUser.FirstName = "mirke";
            updateUser.LastName = "fedee";

            _sharedContext.RefreshIdentityDb();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Users.UpdateUser),
                Content = JsonContent.Create(updateUser)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _sharedContext.IdentityDbContext.Users.Count().Should().Be(4);

            var updatedUser = _sharedContext.IdentityDbContext.Users.Find(_defaultUser.Id);

            updatedUser.UserName.Should().Be("dddssa");
            updatedUser.FirstName.Should().Be("mirke");
            updatedUser.LastName.Should().Be("fedee");

            updatedUser.LastModified.Should().BeAfter(updatedUser.Created);
            updatedUser.LastModifiedBy.Should().Be("default");
        }

        public void Dispose()
        {
            _sharedContext.ResetState();
        }
    }
}
