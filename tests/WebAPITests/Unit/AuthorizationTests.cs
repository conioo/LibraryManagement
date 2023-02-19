using Application.Dtos.Identity.Response;
using CommonContext.SharedContextBuilders;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;
using System.Net.Http.Headers;
using WebAPITests.Integration;

namespace WebAPITests.Unit
{
    public class AuthorizationTests : IClassFixture<AuthorizationContextBuilder>
    {
        private readonly SharedContext _sharedContext;
        private readonly HttpClient _client;

        public readonly LoginResponse _adminResponse;
        public readonly LoginResponse _moderatorResponse;
        public readonly LoginResponse _workerResponse;
        public readonly LoginResponse _basicResponse;

        public AuthorizationTests(AuthorizationContextBuilder contextBuilder)
        {
            _sharedContext = contextBuilder.Value;
            _client = _sharedContext.CreateClient();

            _adminResponse = contextBuilder._adminResponse;
            _moderatorResponse = contextBuilder._moderatorResponse;
            _workerResponse = contextBuilder._workerResponse;
            _basicResponse = contextBuilder._basicResponse;
        }

        //admin to nie admin
        [Theory]
        [MemberData(nameof(AuthorizationData.CorrectAuthorizationForRoleBasic), MemberType = typeof(AuthorizationData))]
        public async Task AuthorizationForRoleBasic_ForCorrectEndpoints_CorrectlyAuthorize(string prefix, string patch, HttpMethod method)
        {
            var request = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri($"{_client.BaseAddress}{prefix}/{patch}"),
            };

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _basicResponse.Jwt);

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        [Theory]
        [MemberData(nameof(AuthorizationData.InCorrectAuthorizationForRoleBasic), MemberType = typeof(AuthorizationData))]
        public async Task AuthorizationForRoleBasic_ForInCorrectEndpoints_InCorrectlyAuthorize(string prefix, string patch, HttpMethod method)
        {
            var request = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri($"{_client.BaseAddress}{prefix}/{patch}"),
            };

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _basicResponse.Jwt);

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }


        [Theory]
        [MemberData(nameof(AuthorizationData.CorrectAuthorizationForRoleWorker), MemberType = typeof(AuthorizationData))]
        public async Task AuthorizationForRoleWorker_ForCorrectEndpoints_CorrectlyAuthorize(string prefix, string patch, HttpMethod method)
        {
            var request = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri($"{_client.BaseAddress}{prefix}/{patch}"),
            };

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _workerResponse.Jwt);

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        [Theory]
        [MemberData(nameof(AuthorizationData.InCorrectAuthorizationForRoleWorker), MemberType = typeof(AuthorizationData))]
        public async Task AuthorizationForRoleWorker_ForInCorrectEndpoints_InCorrectlyAuthorize(string prefix, string patch, HttpMethod method)
        {
            var request = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri($"{_client.BaseAddress}{prefix}/{patch}"),
            };

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _workerResponse.Jwt);

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [MemberData(nameof(AuthorizationData.CorrectAuthorizationForRoleModerator), MemberType = typeof(AuthorizationData))]
        public async Task AuthorizationForRoleModerator_ForCorrectEndpoints_CorrectlyAuthorize(string prefix, string patch, HttpMethod method)
        {
            var request = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri($"{_client.BaseAddress}{prefix}/{patch}"),
            };

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _moderatorResponse.Jwt);

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        [Theory]
        [MemberData(nameof(AuthorizationData.InCorrectAuthorizationForRoleModerator), MemberType = typeof(AuthorizationData))]
        public async Task AuthorizationForRoleModerator_ForInCorrectEndpoints_InCorrectlyAuthorize(string prefix, string patch, HttpMethod method)
        {
            var request = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri($"{_client.BaseAddress}{prefix}/{patch}"),
            };

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _moderatorResponse.Jwt);

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        [Theory]
        [MemberData(nameof(AuthorizationData.CorrectAuthorizationForRoleAdmin), MemberType = typeof(AuthorizationData))]
        public async Task AuthorizationForRoleAdmin_ForCorrectEndpoints_CorrectlyAuthorize(string prefix, string patch, HttpMethod method)
        {
            var request = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri($"{_client.BaseAddress}{prefix}/{patch}"),
            };

            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _adminResponse.Jwt);

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

    }
}
