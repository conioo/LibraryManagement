using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Dtos;
using Application.Interfaces;
using CommonContext;
using CommonContext.SharedContextBuilders;
using FluentAssertions;
using Infrastructure.Identity.Entities;
using Moq;
using System.Net.Http.Json;
using WebAPI.ApiRoutes;
using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Mvc;
using WebAPITests.Integration.SharedContextBuilders;

namespace WebAPITests.Integration
{
    public class AdminControllerTests : IClassFixture<AdminContextBuilder>, IDisposable
    {
        private readonly SharedContext _sharedContext;
        private readonly HttpClient _client;
        private readonly List<ApplicationUser> _allUsers;
        private readonly ApplicationUser _defaultUser;

        public AdminControllerTests(AdminContextBuilder contextBuilder)
        {
            _sharedContext = contextBuilder.Value;
            _client = _sharedContext.CreateClient();

            _defaultUser = DataGenerator.Get<ApplicationUser>(1).First();
            _defaultUser.UserName = "default";

            _sharedContext.UserManager.CreateAsync(_defaultUser, DataGenerator.GetUserPassword);

            _allUsers = _sharedContext.IdentityDbContext.Users.Select(user => user).ToList();
        }

        public void Dispose()
        {
            _sharedContext.IdentityDbContext.Database.EnsureDeleted();
            _sharedContext.IdentityDbContext.Database.EnsureCreated();

            var emailService = _sharedContext.GetMock<IEmailService>();
            emailService.Reset();
        }

        [Fact]
        async Task AddAdminAsync_ForValidModel_Returns200Ok()
        {
            var registerRequest = DataGenerator.Get<RegisterRequest>(1).First();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Admin.AddAdmin),
                Content = JsonContent.Create(registerRequest)
            };

            var response = await _client.SendAsync(request);

            var responseUser = await response.Content.ReadFromJsonAsync<UserResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Users.Prefix}/{Users.GetUser}");

            var emailMock = _sharedContext.GetMock<IEmailService>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            emailMock.Verify(service => service.SendAsync(It.Is<Email>(email => email.To == registerRequest.Email && email.Subject == "Confirm your account" && email.Body.Count() > 0)), Times.Once);
            
            response.Headers.Location.Should().Be(expectedLocationUri);

            _sharedContext.IdentityDbContext.Users.Count().Should().Be(_allUsers.Count + 1);

            var newUser = _sharedContext.IdentityDbContext.Users.Find(responseUser.Id);

            newUser.Should().BeEquivalentTo(registerRequest, options => options.ExcludingMissingMembers());
            newUser.Created.Should().NotBe(null);

            (await _sharedContext.UserManager.IsInRoleAsync(newUser, UserRoles.Admin)).Should().Be(true);
        }

        [Fact]
        async Task AddAdminAsync_ForInValidModel_Returns400BadRequest()
        {
            var registerRequest = DataGenerator.Get<RegisterRequest>(1).First();

            registerRequest.ConfirmPassword = "null";
            registerRequest.Email = "null";
            registerRequest.UserName = String.Empty;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Admin.AddAdmin),
                Content = JsonContent.Create(registerRequest)
            };

            var response = await _client.SendAsync(request);

            var emailMock = _sharedContext.GetMock<IEmailService>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(3);

            emailMock.Verify(service => service.SendAsync(It.IsAny<Email>()), Times.Never);

            _sharedContext.IdentityDbContext.Users.Count().Should().Be(_allUsers.Count);
        }

        [Fact]
        async Task AddWorkerAsync_ForValidModel_Returns200Ok()
        {
            var registerRequest = DataGenerator.Get<RegisterRequest>(1).First();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Admin.AddWorker),
                Content = JsonContent.Create(registerRequest)
            };

            var response = await _client.SendAsync(request);

            var responseUser = await response.Content.ReadFromJsonAsync<UserResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Users.Prefix}/{Users.GetUser}");

            var emailMock = _sharedContext.GetMock<IEmailService>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            emailMock.Verify(service => service.SendAsync(It.Is<Email>(email => email.To == registerRequest.Email && email.Subject == "Confirm your account" && email.Body.Count() > 0)), Times.Once);

            response.Headers.Location.Should().Be(expectedLocationUri);

            _sharedContext.IdentityDbContext.Users.Count().Should().Be(_allUsers.Count + 1);

            var newUser = _sharedContext.IdentityDbContext.Users.Find(responseUser.Id);

            newUser.Should().BeEquivalentTo(registerRequest, options => options.ExcludingMissingMembers());
            newUser.Created.Should().NotBe(null);

            (await _sharedContext.UserManager.IsInRoleAsync(newUser, UserRoles.Worker)).Should().Be(true);
        }

        [Fact]
        async Task AddWorkerAsync_ForInValidModel_Returns400BadRequest()
        {
            var registerRequest = DataGenerator.Get<RegisterRequest>(1).First();

            registerRequest.ConfirmPassword = "null";
            registerRequest.Email = "null";
            registerRequest.UserName = String.Empty;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Admin.AddWorker),
                Content = JsonContent.Create(registerRequest)
            };

            var response = await _client.SendAsync(request);

            var emailMock = _sharedContext.GetMock<IEmailService>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(3);

            emailMock.Verify(service => service.SendAsync(It.IsAny<Email>()), Times.Never);

            _sharedContext.IdentityDbContext.Users.Count().Should().Be(_allUsers.Count);
        }

    }
}
