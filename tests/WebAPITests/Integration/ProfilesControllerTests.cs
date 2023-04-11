using Application.Dtos.Request;
using Application.Dtos.Response;
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
    public class ProfilesControllerTests : IClassFixture<ProfilesContextBuilder>, IDisposable
    {
        private readonly SharedContext _sharedContext;
        private readonly HttpClient _client;
        private readonly ApplicationUser _defaultUser;
        private readonly ApplicationUser _basicUser;
        private readonly Profile _profile;

        public ProfilesControllerTests(ProfilesContextBuilder sharedContextBuilder)
        {
            _sharedContext = sharedContextBuilder.Value;
            _client = _sharedContext.CreateClient();

            _defaultUser = _sharedContext.GetDefaultUser();

            _basicUser = _sharedContext.GetBasicConfirmUser().Result;

            var profile = new Profile()
            {
                UserId = _basicUser.Id,
            };

            var rentals = DataGenerator.Get<Rental>(1);
            var reservations = DataGenerator.Get<Reservation>(1);

           // profile.HistoryRentals = (ICollection<Rental>)rentals;
           // profile.HistoryReservations = (ICollection<Reservation>)reservations;

            _sharedContext.DbContext.Set<Profile>().Add(profile);

            _sharedContext.DbContext.SaveChangesAsync().Wait();

            _profile = profile;
        }

        public void Dispose()
        {
            _sharedContext.DbContext.Database.EnsureDeleted();
            _sharedContext.DbContext.Database.EnsureCreated();

            _sharedContext.IdentityDbContext.Database.EnsureDeleted();
            _sharedContext.IdentityDbContext.Database.EnsureCreated();
        }

        [Fact]
        async Task CreateProfileAsync_ForValidModel_Returns201Created()
        {
            var profileRequest = new ProfileRequest()
            {
                PhoneNumber = "100 200 300"
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Profiles.CreateProfile),
                Content = JsonContent.Create(profileRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            var responseProfile = await response.Content.ReadFromJsonAsync<ProfileResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Profiles.Prefix}/{Profiles.GetProfileByCardNumber}?card-number={responseProfile.LibraryCardNumber}");

            response.Headers.Location.Should().Be(expectedLocationUri);

            responseProfile.UserId.Should().Be(_defaultUser.Id);
            responseProfile.IsActive.Should().BeFalse();

            _sharedContext.DbContext.Set<Profile>().Count().Should().Be(2);

            var profile = _sharedContext.DbContext.Set<Profile>().Find(responseProfile.LibraryCardNumber);

            profile.IsActive.Should().BeFalse();
            profile.UserId.Should().Be(_defaultUser.Id);
            profile.CreatedBy.Should().Be("default");
            profile.LastModifiedBy.Should().Be("default");

            profile.Created.Should().NotBe(null);
            profile.LastModified.Should().NotBe(null);

            _sharedContext.RefreshIdentityDb();

            var defaultUser = _sharedContext.IdentityDbContext.Users.Find(_defaultUser.Id);

            defaultUser.PhoneNumber.Should().Be("100 200 300");
            defaultUser.ProfileId.Should().Be(profile.LibraryCardNumber);
        }

        [Fact]
        async Task CreateProfileAsync_ForExistingProfile_Returns400BadRequest()
        {
            var profile = new Profile()
            {
                UserId = _defaultUser.Id,
            };

            _sharedContext.DbContext.Set<Profile>().Add(profile);

            await _sharedContext.DbContext.SaveChangesAsync();

            var defaultUser = _sharedContext.IdentityDbContext.Users.Find(_defaultUser.Id);

            defaultUser.ProfileId = profile.LibraryCardNumber;

            _sharedContext.IdentityDbContext.Update(defaultUser);
            _sharedContext.IdentityDbContext.SaveChanges();

            var profileRequest = new ProfileRequest()
            {
                PhoneNumber = "100 200 300"
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Profiles.CreateProfile),
                Content = JsonContent.Create(profileRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            _sharedContext.DbContext.Set<Profile>().Count().Should().Be(2);
        }

        [Fact]
        async Task CreateProfileAsync_ForInvalidModel_Returns400BadRequest()
        {
            var profileRequest = new ProfileRequest()
            {
                PhoneNumber = ""
            };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Profiles.CreateProfile),
                Content = JsonContent.Create(profileRequest)
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var details = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            details.Errors.Count().Should().Be(1);

            _sharedContext.DbContext.Set<Profile>().Count().Should().Be(1);
        }

        [Fact]
        async Task ActivationProfileAsync_ForExistingProfile_Returns200Ok()
        {
            var cardNumber = _profile.LibraryCardNumber;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Profiles.ActivationProfile),
                Content = JsonContent.Create(cardNumber)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var profile = _sharedContext.DbContext.Set<Profile>().Find(_profile.LibraryCardNumber);

            profile.IsActive.Should().BeTrue();

            profile.LastModified.Should().BeAfter(profile.Created);
        }

        [Fact]
        async Task ActivationProfileAsync_ForActiveProfile_Returns400BadRequest()
        {
            var profile = _sharedContext.DbContext.Set<Profile>().Find(_profile.LibraryCardNumber);

            profile.IsActive = true;

            _sharedContext.DbContext.Set<Profile>().Update(profile);
            _sharedContext.DbContext.SaveChangesAsync().Wait();


            var cardNumber = _profile.LibraryCardNumber;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Profiles.ActivationProfile),
                Content = JsonContent.Create(cardNumber)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("Profile is already active");

            profile.IsActive.Should().BeTrue();
        }

        [Fact]
        async Task ActivationProfileAsync_ForInvalidCardNumber_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Profiles.ActivationProfile),
                Content = JsonContent.Create("null_null")
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var profile = _sharedContext.DbContext.Set<Profile>().Find(_profile.LibraryCardNumber);

            profile.IsActive.Should().BeFalse();

            profile.LastModified.Should().Be(profile.Created);
        }

        [Fact]
        async Task GetProfileByCardNumberAsync_ForValidCardNumber_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetProfileByCardNumber + $"?card-number={_profile.LibraryCardNumber}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var responseProfile = await response.Content.ReadFromJsonAsync<ProfileResponse>();

            responseProfile.UserId.Should().Be(_basicUser.Id);
            responseProfile.IsActive.Should().BeFalse();
        }

        [Fact]
        async Task GetProfileByCardNumberAsync_ForInvalidCardNumber_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetProfileByCardNumber + $"?card-number=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }
    }
}
