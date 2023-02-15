using Application.Dtos;
using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Interfaces;
using CommonContext;
using CommonContext.SharedContextBuilders;
using FluentAssertions;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using WebAPI.ApiRoutes;

namespace WebAPITests.Integration
{
    public class AccountsControllerTests : IClassFixture<AccountContextBuilder>, IDisposable
    {
        private readonly SharedContext _sharedContext;
        private readonly HttpClient _client;

        public AccountsControllerTests(AccountContextBuilder sharedContextBuilder)
        {
            _sharedContext = sharedContextBuilder.Value;
            _client = _sharedContext.CreateClient();
        }

        public void Dispose()
        {
            _sharedContext.IdentityDbContext.Database.EnsureDeleted();
            _sharedContext.IdentityDbContext.Database.EnsureCreated();

            var emailService = _sharedContext.GetMock<IEmailService>();
            emailService.Reset();
        }

        private async Task<ApplicationUser> GetConfirmUser()
        {
            var user = DataGenerator.Get<ApplicationUser>(1).First();

            await _sharedContext.UserManager.CreateAsync(user, DataGenerator.GetUserPassword);

            var token = await _sharedContext.UserManager.GenerateEmailConfirmationTokenAsync(user);

            await _sharedContext.UserManager.AddToRoleAsync(user, UserRoles.Basic);
            await _sharedContext.UserManager.ConfirmEmailAsync(user, token);

            _sharedContext.RefreshIdentityDb();

            return user;
        }

        private void CheckJwt(string jwt)
        {
            _sharedContext.JwtService.IsActive(jwt).Result.Should().BeTrue();

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenValidatorParameters = _sharedContext.Configuration.GetSection("TokenValidationParameters").Get<TokenValidationParameters>();
            tokenValidatorParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_sharedContext.Configuration.GetSection("Jwt").GetValue<string>("IssuerSigningKey")));
            tokenValidatorParameters.ValidIssuer = _sharedContext.Configuration.GetSection("Jwt").GetValue<string>("Issuer");
            tokenValidatorParameters.ValidAudience = _sharedContext.Configuration.GetSection("Jwt").GetValue<string>("Audience");

            tokenHandler.ValidateToken(jwt, tokenValidatorParameters, out var validated);
        }

        [Fact]
        async Task RegisterAsync_ForValidModel_SuccessRegisterUser()
        {
            var registerRequest = DataGenerator.Get<RegisterRequest>(1).First();
            var numberUsersBefore = _sharedContext.IdentityDbContext.Users.Count();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Register),
                Content = JsonContent.Create(registerRequest)
            };

            var response = await _client.SendAsync(request);

            var responseUser = await response.Content.ReadFromJsonAsync<UserResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Users.Prefix}/{Users.GetUser}");

            var emailMock = _sharedContext.GetMock<IEmailService>();

            emailMock.Verify(service => service.SendAsync(It.Is<Email>(email => email.To == registerRequest.Email && email.Subject == "Confirm your account" && email.Body.Count() > 0)), Times.Once);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().Be(expectedLocationUri);

            _sharedContext.IdentityDbContext.Users.Count().Should().Be(numberUsersBefore + 1);

            var newUser = _sharedContext.IdentityDbContext.Users.Find(responseUser.Id);

            newUser.Should().BeEquivalentTo(registerRequest, options => options.ExcludingMissingMembers());
            newUser.Created.Should().NotBe(null);
        }
        [Fact]
        async Task RegisterAsync_ForInvalidModel_Returns400BadRequest()
        {
            var registerRequest = DataGenerator.Get<RegisterRequest>(1).First();

            registerRequest.Email = "";

            var numberUsersBefore = _sharedContext.IdentityDbContext.Users.Count();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Register),
                Content = JsonContent.Create(registerRequest)
            };

            var response = await _client.SendAsync(request);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            var emailMock = _sharedContext.GetMock<IEmailService>();

            emailMock.Verify(service => service.SendAsync(It.IsAny<Email>()), Times.Never);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            details.Errors.Count().Should().Be(1);

            _sharedContext.IdentityDbContext.Users.Count().Should().Be(numberUsersBefore);
        }
        [Fact]
        async Task RegisterAsync_ForDuplicateEmail_Returns400BadRequest()
        {
            var user = DataGenerator.Get<ApplicationUser>(1).First();

            await _sharedContext.UserManager.CreateAsync(user);

            var registerRequest = DataGenerator.Get<RegisterRequest>(1).First();

            registerRequest.Email = user.Email;
            var numberUsersBefore = _sharedContext.IdentityDbContext.Users.Count();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Register),
                Content = JsonContent.Create(registerRequest)
            };

            var response = await _client.SendAsync(request);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            var emailMock = _sharedContext.GetMock<IEmailService>();

            problemDetails.Errors.Count().Should().Be(1);
            emailMock.Verify(service => service.SendAsync(It.IsAny<Email>()), Times.Never);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            _sharedContext.IdentityDbContext.Users.Count().Should().Be(numberUsersBefore);
        }
        [Fact]
        async Task RegisterAsync_ForDuplicateUsername_Returns400BadRequest()
        {
            var user = DataGenerator.Get<ApplicationUser>(1).First();

            await _sharedContext.UserManager.CreateAsync(user);

            var registerRequest = DataGenerator.Get<RegisterRequest>(1).First();

            registerRequest.UserName = user.UserName;
            var numberUsersBefore = _sharedContext.IdentityDbContext.Users.Count();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Register),
                Content = JsonContent.Create(registerRequest)
            };

            var response = await _client.SendAsync(request);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            var emailMock = _sharedContext.GetMock<IEmailService>();

            problemDetails.Errors.Count().Should().Be(1);
            emailMock.Verify(service => service.SendAsync(It.IsAny<Email>()), Times.Never);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            _sharedContext.IdentityDbContext.Users.Count().Should().Be(numberUsersBefore);
        }
        [Fact]
        async Task ConfirmEmailAsync_ForValidUserIdAndToken_Returns200OK()
        {
            var user = DataGenerator.Get<ApplicationUser>(1).First();

            await _sharedContext.UserManager.CreateAsync(user);

            var userId = _sharedContext.IdentityDbContext.Users.Single(appUser => appUser.Email == user.Email).Id;
            var token = await _sharedContext.UserManager.GenerateEmailConfirmationTokenAsync(user);

            var query = new Dictionary<string, string?>();

            query.Add("userid", userId);
            query.Add("token", token);

            var uri = QueryHelpers.AddQueryString(Accounts.ConfirmEmail, query);

            var response = await _client.GetAsync(uri);

            _sharedContext.RefreshIdentityDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.IdentityDbContext.Users.Single(appUser => appUser.Email == user.Email).EmailConfirmed.Should().BeTrue();
        }
        [Fact]
        async Task ConfirmEmailAsync_ForInValidUserId_Returns404NotFound()
        {
            var user = DataGenerator.Get<ApplicationUser>(1).First();

            await _sharedContext.UserManager.CreateAsync(user);

            var userId = "null-null";
            var token = await _sharedContext.UserManager.GenerateEmailConfirmationTokenAsync(user);

            var query = new Dictionary<string, string?>();

            query.Add("userid", userId);
            query.Add("token", token);

            var uri = QueryHelpers.AddQueryString(Accounts.ConfirmEmail, query);

            var response = await _client.GetAsync(uri);

            _sharedContext.RefreshIdentityDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            _sharedContext.IdentityDbContext.Users.Single(appUser => appUser.Email == user.Email).EmailConfirmed.Should().BeFalse();
        }
        [Fact]
        async Task ConfirmEmailAsync_ForInValidToken_Returns400BadRequest()
        {
            var user = DataGenerator.Get<ApplicationUser>(1).First();

            await _sharedContext.UserManager.CreateAsync(user);

            var userId = _sharedContext.IdentityDbContext.Users.Single(appUser => appUser.Email == user.Email).Id;
            var token = "null-token-null";

            var query = new Dictionary<string, string?>();

            query.Add("userid", userId);
            query.Add("token", token);

            var uri = QueryHelpers.AddQueryString(Accounts.ConfirmEmail, query);

            var response = await _client.GetAsync(uri);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            _sharedContext.RefreshIdentityDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            details.Errors.Count().Should().Be(1);
            _sharedContext.IdentityDbContext.Users.Single(appUser => appUser.Email == user.Email).EmailConfirmed.Should().BeFalse();
        }
        [Fact]
        async Task LoginAsync_ForValidEmailAndPassword_Returns200Ok()
        {
            var user = await GetConfirmUser();

            var loginRequest = new LoginRequest();

            loginRequest.Email = user.Email;
            loginRequest.Password = DataGenerator.GetUserPassword;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Login),
                Content = JsonContent.Create(loginRequest)
            };

            var response = await _client.SendAsync(request);

            var responseLogin = await response.Content.ReadFromJsonAsync<LoginResponse>();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var tokenModel = _sharedContext.IdentityDbContext.RefreshTokens.Include(token => token.ApplicationUser).Single(tokenModel => tokenModel.Token == responseLogin.RefreshToken);

            responseLogin.UserId.Should().Be(user.Id);
            tokenModel.IsActive.Should().BeTrue();

            tokenModel.ApplicationUser.Id.Should().Be(user.Id);

            CheckJwt(responseLogin.Jwt);
        }
        [Fact]
        async Task LoginAsync_ForInValidEmail_Returns401Unauthorized()
        {
            var user = await GetConfirmUser();

            var loginRequest = new LoginRequest();

            loginRequest.Email = "null@example.com";
            loginRequest.Password = DataGenerator.GetUserPassword;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Login),
                Content = JsonContent.Create(loginRequest)
            };

            var response = await _client.SendAsync(request);

            var responseMessage = await response.Content.ReadAsStringAsync();

            responseMessage.Should().Be("Invalid email or password");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }
        [Fact]
        async Task LoginAsync_ForInValidPassword_Returns401Unauthorized()
        {
            var user = await GetConfirmUser();

            var loginRequest = new LoginRequest();

            loginRequest.Email = user.Email;
            loginRequest.Password = "null_null";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Login),
                Content = JsonContent.Create(loginRequest)
            };

            var response = await _client.SendAsync(request);

            var responseMessage = await response.Content.ReadAsStringAsync();

            responseMessage.Should().Be("Invalid email or password");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }
        [Fact]
        async Task LoginAsync_ForInValidEmailAndPassword_Returns401Unauthorized()
        {
            var user = await GetConfirmUser();

            var loginRequest = new LoginRequest();

            loginRequest.Email = "null@example.com";
            loginRequest.Password = "null_null";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Login),
                Content = JsonContent.Create(loginRequest)
            };

            var response = await _client.SendAsync(request);

            var responseMessage = await response.Content.ReadAsStringAsync();

            responseMessage.Should().Be("Invalid email or password");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }
        [Fact]
        async Task LoginAsync_ForInValidModel_Returns400BadRequest()
        {
            var user = await GetConfirmUser();

            var loginRequest = new LoginRequest();

            loginRequest.Email = "nullInvalid";
            loginRequest.Password = "";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Login),
                Content = JsonContent.Create(loginRequest)
            };

            var response = await _client.SendAsync(request);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(2);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
        [Fact]
        async Task LogoutAsync_ForValid_Returns200Ok()
        {
            var user = await GetConfirmUser();

            var loginRequest = new LoginRequest();

            loginRequest.Email = user.Email;
            loginRequest.Password = DataGenerator.GetUserPassword;

            var requestLogin = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Login),
                Content = JsonContent.Create(loginRequest)
            };

            var response = await _client.SendAsync(requestLogin);

            var responseLogin = await response.Content.ReadFromJsonAsync<LoginResponse>();


            var requestLogout = new HttpRequestMessage()
            {
                Method = HttpMethod.Head,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Logout),
            };

            requestLogout.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, responseLogin.Jwt);

            response = await _client.SendAsync(requestLogout);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            _sharedContext.JwtService.IsActive(responseLogin.Jwt).Result.Should().BeFalse();
            _sharedContext.IdentityDbContext.RefreshTokens.Single(token => token.Token == responseLogin.RefreshToken).IsActive.Should().BeFalse();
        }
        [Fact]
        async Task RefreshTokenAsync_ForValidToken_ReturnsValidJwt()
        {
            var user = await GetConfirmUser();

            var loginRequest = new LoginRequest();

            loginRequest.Email = user.Email;
            loginRequest.Password = DataGenerator.GetUserPassword;

            var requestLogin = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Login),
                Content = JsonContent.Create(loginRequest)
            };

            var response = await _client.SendAsync(requestLogin);
            var responseLogin = await response.Content.ReadFromJsonAsync<LoginResponse>();

            var query = new Dictionary<string, string?>();

            query.Add("refresh_token", responseLogin.RefreshToken);

            var uri = QueryHelpers.AddQueryString(Accounts.RefreshToken, query);

            response = await _client.GetAsync(uri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var newJwt = await response.Content.ReadAsStringAsync();

            CheckJwt(newJwt);
        }

        [Fact]
        async Task ForgotPasswordAsync_ForValidEmail_Returns200Ok()
        {
            var user = await GetConfirmUser();

            var forgotPasswordRequest = new ForgotPasswordRequest();
            forgotPasswordRequest.Email = user.Email;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.ForgotPassword),
                Content = JsonContent.Create(forgotPasswordRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var emailMock = _sharedContext.GetMock<IEmailService>();

            emailMock.Verify(service => service.SendAsync(It.Is<Email>(email => email.Subject == "Reset Password" && email.Body != string.Empty)), Times.Once);
        }

        [Fact]
        async Task ForgotPasswordAsync_ForInValidEmail_Returns400BadRequest()
        {
            var forgotPasswordRequest = new ForgotPasswordRequest();
            forgotPasswordRequest.Email = "@@@7@osososs";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.ForgotPassword),
                Content = JsonContent.Create(forgotPasswordRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count().Should().Be(1);

            var emailMock = _sharedContext.GetMock<IEmailService>();

            emailMock.Verify(service => service.SendAsync(It.IsAny<Email>()), Times.Never);
        }

        [Fact]
        async Task ForgotPasswordAsync_ForValidEmailButNoUser_Returns404NotFound()
        {
            var forgotPasswordRequest = new ForgotPasswordRequest();
            forgotPasswordRequest.Email = "example@example.com";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.ForgotPassword),
                Content = JsonContent.Create(forgotPasswordRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var emailMock = _sharedContext.GetMock<IEmailService>();

            emailMock.Verify(service => service.SendAsync(It.IsAny<Email>()), Times.Never);
        }

        [Fact]
        async Task ResetPasswordAsync_ForValidModel_Returns200Ok()
        {
            var user = await GetConfirmUser();

            var resetPasswordToken = await _sharedContext.UserManager.GeneratePasswordResetTokenAsync(user);

            var resetPasswordRequest = new ResetPasswordRequest();

            resetPasswordRequest.Email = user.Email;
            resetPasswordRequest.PasswordResetToken = resetPasswordToken;
            resetPasswordRequest.NewPassword = DataGenerator.GetOtherUserPassword;
            resetPasswordRequest.ConfirmNewPassword = DataGenerator.GetOtherUserPassword;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.ResetPassword),
                Content = JsonContent.Create(resetPasswordRequest)
            };

            var response = await _client.SendAsync(request);
            _sharedContext.RefreshIdentityDb();

            user = _sharedContext.IdentityDbContext.Users.Find(user.Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, DataGenerator.GetOtherUserPassword);

            result.Should().Be(PasswordVerificationResult.Success);
        }

        [Fact]
        async Task ResetPasswordAsync_ForInValidModel_Returns400BadRequest()
        {
            var user = await GetConfirmUser();

            var resetPasswordToken = await _sharedContext.UserManager.GeneratePasswordResetTokenAsync(user);

            var resetPasswordRequest = new ResetPasswordRequest();

            resetPasswordRequest.Email = "nwjxx";
            resetPasswordRequest.PasswordResetToken = resetPasswordToken;
            resetPasswordRequest.NewPassword = DataGenerator.GetOtherUserPassword;
            resetPasswordRequest.ConfirmNewPassword = "null";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.ResetPassword),
                Content = JsonContent.Create(resetPasswordRequest)
            };

            var response = await _client.SendAsync(request);
            _sharedContext.RefreshIdentityDb();

            user = _sharedContext.IdentityDbContext.Users.Find(user.Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count().Should().Be(2);


            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, DataGenerator.GetUserPassword);

            result.Should().Be(PasswordVerificationResult.Success);
        }

        [Fact]
        async Task ResetPasswordAsync_ForInValidResetToken_Returns400BadRequest()
        {
            var user = await GetConfirmUser();

            var resetPasswordRequest = new ResetPasswordRequest();

            resetPasswordRequest.Email = user.Email;
            resetPasswordRequest.PasswordResetToken = "token";
            resetPasswordRequest.NewPassword = DataGenerator.GetOtherUserPassword;
            resetPasswordRequest.ConfirmNewPassword = DataGenerator.GetOtherUserPassword;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.ResetPassword),
                Content = JsonContent.Create(resetPasswordRequest)
            };

            var response = await _client.SendAsync(request);
            _sharedContext.RefreshIdentityDb();

            user = _sharedContext.IdentityDbContext.Users.Find(user.Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count().Should().Be(1);

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, DataGenerator.GetUserPassword);

            result.Should().Be(PasswordVerificationResult.Success);
        }

        [Fact]
        async Task ResetPasswordAsync_ForValidEmailButNoUser_Returns404NotFound()
        {
            var resetPasswordRequest = new ResetPasswordRequest();

            resetPasswordRequest.Email = "example@example.com";
            resetPasswordRequest.PasswordResetToken = "token";
            resetPasswordRequest.NewPassword = DataGenerator.GetOtherUserPassword;
            resetPasswordRequest.ConfirmNewPassword = DataGenerator.GetOtherUserPassword;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.ResetPassword),
                Content = JsonContent.Create(resetPasswordRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task ChangePasswordAsync_ForValidModel_Returns200Ok()
        {
            var user = await GetConfirmUser();

            var loginRequest = new LoginRequest();

            loginRequest.Email = user.Email;
            loginRequest.Password = DataGenerator.GetUserPassword;

            var requestLogin = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Login),
                Content = JsonContent.Create(loginRequest)
            };

            var response = await _client.SendAsync(requestLogin);
            var responseLogin = await response.Content.ReadFromJsonAsync<LoginResponse>();

            var changePassword = new ChangePasswordRequest();

            changePassword.OldPassword = DataGenerator.GetUserPassword;
            changePassword.NewPassword = DataGenerator.GetOtherUserPassword;
            changePassword.ConfirmNewPassword = DataGenerator.GetOtherUserPassword;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.ChangePassword),
                Content = JsonContent.Create(changePassword)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, responseLogin.Jwt);

            response = await _client.SendAsync(request);
            _sharedContext.RefreshIdentityDb();

            user = _sharedContext.IdentityDbContext.Users.Find(user.Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, DataGenerator.GetOtherUserPassword);

            result.Should().Be(PasswordVerificationResult.Success);
        }

        [Fact]
        async Task ChangePasswordAsync_ForInValidModel_Returns400BadRequest()
        {
            var user = await GetConfirmUser();

            var loginRequest = new LoginRequest();

            loginRequest.Email = user.Email;
            loginRequest.Password = DataGenerator.GetUserPassword;

            var requestLogin = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Login),
                Content = JsonContent.Create(loginRequest)
            };

            var response = await _client.SendAsync(requestLogin);
            var responseLogin = await response.Content.ReadFromJsonAsync<LoginResponse>();

            var changePassword = new ChangePasswordRequest();

            changePassword.OldPassword = DataGenerator.GetUserPassword;
            changePassword.NewPassword = DataGenerator.GetOtherUserPassword;
            changePassword.ConfirmNewPassword = "password";

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.ChangePassword),
                Content = JsonContent.Create(changePassword)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, responseLogin.Jwt);

            response = await _client.SendAsync(request);
            _sharedContext.RefreshIdentityDb();

            user = _sharedContext.IdentityDbContext.Users.Find(user.Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count().Should().Be(1);

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, DataGenerator.GetUserPassword);

            result.Should().Be(PasswordVerificationResult.Success);
        }

        [Fact]
        async Task ChangePasswordAsync_ForInValidOldPassword_Returns400BadRequest()
        {
            var user = await GetConfirmUser();

            var loginRequest = new LoginRequest();

            loginRequest.Email = user.Email;
            loginRequest.Password = DataGenerator.GetUserPassword;

            var requestLogin = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.Login),
                Content = JsonContent.Create(loginRequest)
            };

            var response = await _client.SendAsync(requestLogin);
            var responseLogin = await response.Content.ReadFromJsonAsync<LoginResponse>();

            var changePassword = new ChangePasswordRequest();

            changePassword.OldPassword = "password";
            changePassword.NewPassword = DataGenerator.GetOtherUserPassword;
            changePassword.ConfirmNewPassword = DataGenerator.GetOtherUserPassword;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Accounts.ChangePassword),
                Content = JsonContent.Create(changePassword)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, responseLogin.Jwt);

            response = await _client.SendAsync(request);
            _sharedContext.RefreshIdentityDb();

            user = _sharedContext.IdentityDbContext.Users.Find(user.Id);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            details.Errors.Count().Should().Be(1);

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, DataGenerator.GetUserPassword);

            result.Should().Be(PasswordVerificationResult.Success);
        }
    }
}
