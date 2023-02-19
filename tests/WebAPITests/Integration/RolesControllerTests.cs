using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Dtos.Response;
using CommonContext;
using CommonContext.SharedContextBuilders;
using FluentAssertions;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Sieve.Models;
using System.Net;
using System.Net.Http.Json;
using WebAPI.ApiRoutes;

namespace WebAPITests.Integration
{
    public class RolesControllerTests : IClassFixture<RoleContextBuilder>, IDisposable
    {
        private readonly SharedContext _sharedContext;
        private readonly HttpClient _client;
        private readonly List<IdentityRole> _roles;

        private readonly int _defaultRolesCount;
        private readonly ApplicationUser _defaultUser;
        private List<ApplicationUser>? _allUsers;
        private List<ApplicationUser>? _basicUsers;

        public RolesControllerTests(RoleContextBuilder contextBuilder)
        {
            _sharedContext = contextBuilder.Value;
            _client = _sharedContext.CreateClient();

            _roles = (List<IdentityRole>)DataGenerator.Get<IdentityRole>(3);

            _defaultRolesCount = _sharedContext.IdentityDbContext.Roles.Count() + 3;

            var exsistingRoles = _sharedContext.IdentityDbContext.Roles.Select(role => role).ToArray();

            _sharedContext.IdentityDbContext.Roles.AddRange(_roles);


            _defaultUser = DataGenerator.Get<ApplicationUser>(1).First();
            _defaultUser.UserName = "default";

            _sharedContext.UserManager.CreateAsync(_defaultUser, DataGenerator.GetUserPassword).Wait();

            _sharedContext.IdentityDbContext.SaveChangesAsync();

            _roles.AddRange(exsistingRoles);
        }

        public void Dispose()
        {
            _sharedContext.IdentityDbContext.Database.EnsureDeleted();
            _sharedContext.IdentityDbContext.Database.EnsureCreated();
        }

        private void SeedUsers()
        {
            var admin = _sharedContext.IdentityDbContext.Users.Single(user => user.UserName == "Admin");

            var users = DataGenerator.Get<ApplicationUser>(3);

            _sharedContext.UserManager.CreateAsync(users.ElementAt(0), DataGenerator.GetUserPassword).Wait();
            _sharedContext.UserManager.CreateAsync(users.ElementAt(1), DataGenerator.GetUserPassword).Wait();
            _sharedContext.UserManager.CreateAsync(users.ElementAt(2), DataGenerator.GetUserPassword).Wait();

            _sharedContext.UserManager.AddToRoleAsync(users.ElementAt(0), UserRoles.Basic).Wait();
            _sharedContext.UserManager.AddToRoleAsync(users.ElementAt(2), UserRoles.Basic).Wait();

            _basicUsers = new List<ApplicationUser>();

            _basicUsers.Add(users.ElementAt(0));
            _basicUsers.Add(users.ElementAt(2));

            _allUsers = (List<ApplicationUser>)users;
            _allUsers.Add(admin);
            _allUsers.Add(_defaultUser);
        }


        [Fact]
        async Task GetAllRolesAsync_ForSevenRoles_ReturnsAllRoles()
        {
            var response = await _client.GetAsync(Roles.GetAllRoles);

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<RoleResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            result.Count().Should().Be(7);

            result.Should().BeEquivalentTo(_roles, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetRoleByIdAsync_ForValidId_ReturnsCorrectRole()
        {
            var requestUri = QueryHelpers.AddQueryString(Roles.GetRoleById, "id", _roles.First().Id);

            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<ItemResponse>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.Should().BeEquivalentTo(_roles.First(), options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetRoleByIdAsync_ForInvalidId_Returns404NotFound()
        {
            var requestUri = QueryHelpers.AddQueryString(Roles.GetRoleById, "id", "null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetPageAsync_ForValidPage_ReturnsCorrectPage()
        {
            var sieveModel = new SieveModel()
            {
                PageSize = 3,
                Page = 3,
            };

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Roles.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<RoleResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.TotalPages.Should().Be(3);
            result.TotalItemsCount.Should().Be(7);
            result.Items.Count().Should().Be(1);

            _roles.Should().ContainEquivalentOf(result.Items.ElementAt(0));
        }

        [Fact]
        async Task GetPageAsync_ForNonExistingPage_ReturnsEmptyPage()
        {
            var sieveModel = new SieveModel()
            {
                PageSize = 2,
                Page = 5,
            };

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Roles.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<ItemResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.TotalPages.Should().Be(4);
            result.TotalItemsCount.Should().Be(7);
            result.Items.Should().BeEmpty();
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

            var requestUri = QueryHelpers.AddQueryString(Roles.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count().Should().Be(2);
        }

        [Fact]
        async Task GetPageAsync_UsingSortedPage_ReturnsCorrectSortedPage()
        {
            var sieveModel = new SieveModel()
            {
                Page = 1,
                PageSize = 3,
                Sorts = "Name"
            };

            var queryString = new Dictionary<string, string?>();
            queryString.Add(nameof(sieveModel.Sorts), sieveModel.Sorts);
            queryString.Add(nameof(sieveModel.Filters), sieveModel.Filters);
            queryString.Add(nameof(sieveModel.Page), sieveModel.Page.ToString());
            queryString.Add(nameof(sieveModel.PageSize), sieveModel.PageSize.ToString());

            var requestUri = QueryHelpers.AddQueryString(Roles.GetPage, queryString);
            var response = await _client.GetAsync(requestUri);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<RoleResponse>>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            result.Items.Count().Should().Be(3);

            result.Items.Should().BeInAscendingOrder(role => role.Name);
        }

        [Fact]
        async Task AddRoleAsync_ForValidModel_Returns201Created()
        {
            var roleRequest = DataGenerator.Get<RoleRequest>(1).First();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Roles.AddRole),
                Content = JsonContent.Create(roleRequest)
            };

            var response = await _client.SendAsync(request);

            var roleResponse = await response.Content.ReadFromJsonAsync<RoleResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Roles.Prefix}/{Roles.GetRoleById}?id={roleResponse.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be(expectedLocationUri);

            _sharedContext.IdentityDbContext.Roles.Count().Should().Be(_defaultRolesCount + 1);

            _sharedContext.IdentityDbContext.Roles.Single(role => role.Name == roleRequest.Name).Should().BeEquivalentTo(roleRequest, options => options.ExcludingMissingMembers());

            roleResponse.Should().BeEquivalentTo(roleRequest, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task AddRoleAsync_ForInvalidModel_Returns400BadRequest()
        {
            var roleRequest = new RoleRequest();

            roleRequest.Name = "al";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Roles.AddRole),
                Content = JsonContent.Create(roleRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(1);

            _sharedContext.IdentityDbContext.Roles.Count().Should().Be(_defaultRolesCount);
        }

        [Fact]
        async Task AddRoleAsync_ForDuplicateRoleName_Returns400BadRequest()
        {
            var roleRequest = new RoleRequest();

            roleRequest.Name = _roles.First().Name;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Roles.AddRole),
                Content = JsonContent.Create(roleRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(1);

            _sharedContext.IdentityDbContext.Roles.Count().Should().Be(_defaultRolesCount);
        }

        [Fact]
        async Task RemoveRoleAsync_ForValidId_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Roles.RemoveRole + $"?id={_roles.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.IdentityDbContext.Roles.Count().Should().Be(_defaultRolesCount - 1);
        }

        [Fact]
        async Task RemoveRoleAsync_ForInvalidId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Roles.RemoveRole + $"?id=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            _sharedContext.IdentityDbContext.Roles.Count().Should().Be(_defaultRolesCount);
        }

        [Fact]
        async Task UpdateRoleAsync_ForValidModel_Returns200Ok()
        {
            var roleRequest = new RoleRequest();

            roleRequest.Name = "changeName";

            _sharedContext.RefreshIdentityDb();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Roles.UpdateRole + $"?id={_roles.First().Id}"),
                Content = JsonContent.Create(roleRequest)
            };

            var response = await _client.SendAsync(request);

            var updatedRole = _sharedContext.IdentityDbContext.Roles.Find(_roles.First().Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.IdentityDbContext.Roles.Count().Should().Be(_defaultRolesCount);

            updatedRole.Should().BeEquivalentTo(roleRequest, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task UpdateItemAsync_ForInvalidModel_Returns400BadRequest()
        {
            var roleRequest = new RoleRequest();

            roleRequest.Name = "al";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_client.BaseAddress + Roles.UpdateRole + $"?id={_roles.First().Id}"),
                Content = JsonContent.Create(roleRequest)
            };

            var response = await _client.SendAsync(request);

            var updatedRole = _sharedContext.IdentityDbContext.Roles.Find(_roles.First().Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            _sharedContext.IdentityDbContext.Roles.Count().Should().Be(_defaultRolesCount);

            updatedRole.Should().BeEquivalentTo(_roles.First(), options => options.ExcludingMissingMembers());

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count().Should().Be(1);
        }

        [Fact]
        async Task GetUsersInRoleAsync_ForValidRoleId_ReturnsTwoBasicUsers()
        {
            SeedUsers();

            var roleBasicId = _sharedContext.IdentityDbContext.Roles.Single(role => role.Name == UserRoles.Basic).Id;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Roles.GetUsersInRole + $"?roleId={roleBasicId}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var userResponse = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>();

            userResponse.Should().BeEquivalentTo(_basicUsers, options => options.ExcludingMissingMembers());
        }

        [Fact]
        async Task GetUsersInRoleAsync_ForValidRoleIdButNoUsers_ReturnsEmptyListUsers()
        {
            SeedUsers();

            var roleBasicId = _sharedContext.IdentityDbContext.Roles.Single(role => role.Name == UserRoles.Moderator).Id;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Roles.GetUsersInRole + $"?roleId={roleBasicId}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var userResponse = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>();

            userResponse.Count().Should().Be(0);
        }
        [Fact]
        async Task GetUsersInRoleAsync_ForInValidRoleId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Roles.GetUsersInRole + $"?roleId=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetRolesByUserAsync_ForValidUserId_ReturnsTwoRoles()
        {
            SeedUsers();

            _sharedContext.UserManager.AddToRoleAsync(_defaultUser, _roles.First().Name).Wait();
            _sharedContext.UserManager.AddToRoleAsync(_defaultUser, _roles.Last().Name).Wait();

            var ExpectedUserRoles = new List<IdentityRole>();

            ExpectedUserRoles.Add(_roles.First());
            ExpectedUserRoles.Add(_roles.Last());

            var roleBasicId = _sharedContext.IdentityDbContext.Roles.Single(role => role.Name == UserRoles.Basic).Id;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Roles.GetRolesByUser + $"?userId={_defaultUser.Id}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var roleResponse = await response.Content.ReadFromJsonAsync<IEnumerable<RoleResponse>>();

            roleResponse.Should().BeEquivalentTo(ExpectedUserRoles, options => options.Excluding(role => role.Id).ExcludingMissingMembers());
        }

        [Fact]
        async Task GetRolesByUserAsync_ForInValidUserId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Roles.GetRolesByUser + $"?userId=null_nul"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        async Task AddUsersToRoleAsync_ForValidRoleId_CorrectAddsRolesToUser()
        {
            SeedUsers();

            var roleModification = new RoleModificationRequest()
            {
                RoleId = _roles.First().Id,
                UsersId = new List<string> { _allUsers[0].Id, _allUsers[1].Id }
            };

            _sharedContext.RefreshScope();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Roles.AddUsersToRole),
                Content = JsonContent.Create(roleModification)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var user_0 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[0].Id);
            var user_1 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[1].Id);

            (await _sharedContext.UserManager.IsInRoleAsync(user_0, _roles.First().Name)).Should().BeTrue();
            (await _sharedContext.UserManager.IsInRoleAsync(user_1, _roles.First().Name)).Should().BeTrue();
        }

        [Fact]
        async Task AddUsersToRoleAsync_ForInValidRoleId_Returns404NotFound()
        {
            SeedUsers();

            var roleModification = new RoleModificationRequest()
            {
                RoleId = "null_null",
                UsersId = new List<string> { _allUsers[0].Id, _allUsers[1].Id }
            };

            _sharedContext.RefreshScope();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Roles.AddUsersToRole),
                Content = JsonContent.Create(roleModification)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var user_0 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[0].Id);
            var user_1 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[1].Id);

            (await _sharedContext.UserManager.IsInRoleAsync(user_0, _roles.First().Name)).Should().BeFalse();
            (await _sharedContext.UserManager.IsInRoleAsync(user_1, _roles.First().Name)).Should().BeFalse();
        }

        [Fact]
        async Task AddUsersToRoleAsync_ForInValidOneUserId_NoAddUsersToRole()
        {
            SeedUsers();

            var roleModification = new RoleModificationRequest()
            {
                RoleId = _roles.First().Id,
                UsersId = new List<string> { _allUsers[0].Id, "null_null" }
            };

            _sharedContext.RefreshScope();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Roles.AddUsersToRole),
                Content = JsonContent.Create(roleModification)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var user_0 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[0].Id);
            var user_1 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[1].Id);

            (await _sharedContext.UserManager.IsInRoleAsync(user_0, _roles.First().Name)).Should().BeFalse();
            (await _sharedContext.UserManager.IsInRoleAsync(user_1, _roles.First().Name)).Should().BeFalse();
        }

        [Fact]
        async Task AddUsersToRoleAsync_ForInValidModel_Returns400BadRequest()
        {
            var roleModification = new RoleModificationRequest()
            {
                RoleId = "",
            };

            _sharedContext.RefreshScope();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Roles.AddUsersToRole),
                Content = JsonContent.Create(roleModification)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(2);
        }

        [Fact]
        async Task RemoveRoleFromUsersAsync_ForValidRoleId_CorrectRemovesRoleFromUsers()
        {
            SeedUsers();

            await _sharedContext.UserManager.AddToRoleAsync(_allUsers[0], _roles.First().Name);
            await _sharedContext.UserManager.AddToRoleAsync(_allUsers[1], _roles.First().Name);

            var roleModification = new RoleModificationRequest()
            {
                RoleId = _roles.First().Id,
                UsersId = new List<string> { _allUsers[0].Id, _allUsers[1].Id }
            };

            _sharedContext.RefreshScope();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Roles.RemoveRoleFromUsers),
                Content = JsonContent.Create(roleModification)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var user_0 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[0].Id);
            var user_1 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[1].Id);

            await _sharedContext.IdentityDbContext.SaveChangesAsync();

            (await _sharedContext.UserManager.IsInRoleAsync(user_0, _roles.First().Name)).Should().BeFalse();
            (await _sharedContext.UserManager.IsInRoleAsync(user_1, _roles.First().Name)).Should().BeFalse();
        }

        [Fact]
        async Task RemoveRoleFromUsersAsync_ForInValidRoleId_Returns404NotFound()
        {
            SeedUsers();

            await _sharedContext.UserManager.AddToRoleAsync(_allUsers[0], _roles.First().Name);
            await _sharedContext.UserManager.AddToRoleAsync(_allUsers[1], _roles.First().Name);

            var roleModification = new RoleModificationRequest()
            {
                RoleId = "null_null",
                UsersId = new List<string> { _allUsers[0].Id, _allUsers[1].Id }
            };

            _sharedContext.RefreshScope();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Roles.RemoveRoleFromUsers),
                Content = JsonContent.Create(roleModification)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var user_0 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[0].Id);
            var user_1 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[1].Id);

            (await _sharedContext.UserManager.IsInRoleAsync(user_0, _roles.First().Name)).Should().BeTrue();
            (await _sharedContext.UserManager.IsInRoleAsync(user_1, _roles.First().Name)).Should().BeTrue();
        }

        [Fact]
        async Task RemoveRoleFromUsersAsync_ForInValidOneUserId_NoRemoveRoleFromUsers()
        {
            SeedUsers();

            await _sharedContext.UserManager.AddToRoleAsync(_allUsers[0], _roles.First().Name);
            await _sharedContext.UserManager.AddToRoleAsync(_allUsers[1], _roles.First().Name);

            var roleModification = new RoleModificationRequest()
            {
                RoleId = _roles.First().Id,
                UsersId = new List<string> { _allUsers[0].Id, "null_null" }
            };

            _sharedContext.RefreshScope();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Roles.RemoveRoleFromUsers),
                Content = JsonContent.Create(roleModification)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var user_0 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[0].Id);
            var user_1 = _sharedContext.IdentityDbContext.Users.Find(_allUsers[1].Id);

            (await _sharedContext.UserManager.IsInRoleAsync(user_0, _roles.First().Name)).Should().BeTrue();
            (await _sharedContext.UserManager.IsInRoleAsync(user_1, _roles.First().Name)).Should().BeTrue();
        }

        [Fact]
        async Task RemoveRoleFromUsersAsync_ForInValidModel_Returns400BadRequest()
        {
            var roleModification = new RoleModificationRequest()
            {
                RoleId = "",
            };

            _sharedContext.RefreshScope();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Roles.RemoveRoleFromUsers),
                Content = JsonContent.Create(roleModification)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(2);
        }


    }
}
