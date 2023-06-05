using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Dtos.Response.Archive;
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
        private readonly Profile _profile;

        public ProfilesControllerTests(ProfilesContextBuilder sharedContextBuilder)
        {
            _sharedContext = sharedContextBuilder.Value;
            _client = _sharedContext.CreateClient();

            _defaultUser = _sharedContext.DefaultUser;
            _profile = _sharedContext.DefaultProfile;
        }

        public void Dispose()
        {
            _sharedContext.ResetState();
        }

        [Fact]
        async Task CreateProfileAsync_ForValidModel_Returns201Created()
        {
            _defaultUser.ProfileCardNumber = null;
            _sharedContext.IdentityDbContext.Update(_defaultUser);
            await _sharedContext.IdentityDbContext.SaveChangesAsync();

            _sharedContext.DbContext.Set<Profile>().Remove(_profile);
            _sharedContext.DbContext.Set<ProfileHistory>().Remove(_profile.ProfileHistory);
            await _sharedContext.DbContext.SaveChangesAsync();

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

            _sharedContext.DbContext.Set<Profile>().Count().Should().Be(1);
            _sharedContext.DbContext.Set<ProfileHistory>().Count().Should().Be(1);

            var profile = _sharedContext.DbContext.Set<Profile>().Find(responseProfile.LibraryCardNumber);

            profile.IsActive.Should().BeFalse();
            profile.UserId.Should().Be(_defaultUser.Id);
            profile.CreatedBy.Should().Be("default");
            profile.LastModifiedBy.Should().Be("default");

            profile.Created.Should().NotBe(null);
            profile.LastModified.Should().Be(profile.Created);

            _sharedContext.RefreshIdentityDb();

            var defaultUser = _sharedContext.IdentityDbContext.Users.Find(_defaultUser.Id);

            defaultUser.PhoneNumber.Should().Be("100 200 300");
            defaultUser.ProfileCardNumber.Should().Be(profile.LibraryCardNumber);
        }

        [Fact]
        async Task CreateProfileAsync_ForExistingProfile_Returns400BadRequest()
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

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            _sharedContext.DbContext.Set<Profile>().Count().Should().Be(1);
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
                Method = HttpMethod.Patch,
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
                Method = HttpMethod.Patch,
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
                Method = HttpMethod.Patch,
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
        async Task DeactivationProfileAsync_ForActiveProfile_Returns200Ok()
        {
            var profile = _sharedContext.DbContext.Set<Profile>().Find(_profile.LibraryCardNumber);

            profile.IsActive = true;

            _sharedContext.DbContext.Set<Profile>().Update(profile);
            _sharedContext.DbContext.SaveChangesAsync().Wait();

            var cardNumber = _profile.LibraryCardNumber;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Profiles.DeactivationProfile),
                Content = JsonContent.Create(cardNumber)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            profile = _sharedContext.DbContext.Set<Profile>().Find(_profile.LibraryCardNumber);

            profile.IsActive.Should().BeFalse();

            profile.LastModified.Should().BeAfter(profile.Created);
        }

        [Fact]
        async Task DeactivationProfileAsync_ForDeactiveProfile_Returns400BadRequest()
        {
            var cardNumber = _profile.LibraryCardNumber;

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Profiles.DeactivationProfile),
                Content = JsonContent.Create(cardNumber)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            var profile = _sharedContext.DbContext.Set<Profile>().Find(_profile.LibraryCardNumber);

            responseString.Should().Be("Profile is already deactive");

            profile.IsActive.Should().BeFalse();
        }

        [Fact]
        async Task DeactivationProfileAsync_ForInvalidCardNumber_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_client.BaseAddress + Profiles.DeactivationProfile),
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
        async Task GetProfileAsync_ForExistingProfile_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetProfile),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var responseProfile = await response.Content.ReadFromJsonAsync<ProfileResponse>();

            responseProfile.Should().BeEquivalentTo(_profile, options =>
                options.AllowingInfiniteRecursion()
                .ExcludingMissingMembers()
                .Excluding(profile => profile.ProfileHistory)
            );

            responseProfile.ProfileHistory.Should().BeNull();

            responseProfile.CurrrentRentals.First().ItemTitle.Should().NotBeNull().And.Be(_profile.CurrrentRentals.First().Copy.Item.Title);
            responseProfile.CurrrentReservations.First().ItemTitle.Should().NotBeNull().And.Be(_profile.CurrrentReservations.First().Copy.Item.Title);
        }

        [Fact]
        async Task GetProfileAsync_ForNonExistingProfile_Returns404NotFound()
        {
            _sharedContext.DbContext.Set<Profile>().Remove(_profile);
            await _sharedContext.DbContext.SaveChangesAsync();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetProfile),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetProfileWithHistory_ForExistingProfile_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetProfileWithHistory),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var responseProfile = await response.Content.ReadFromJsonAsync<ProfileResponse>();

            responseProfile.Should().BeEquivalentTo(_profile, options =>
                options.AllowingInfiniteRecursion()
                .ExcludingMissingMembers()
            );

            responseProfile.CurrrentRentals.First().ItemTitle.Should().NotBeNull().And.Be(_profile.CurrrentRentals.First().Copy.Item.Title);
            responseProfile.CurrrentReservations.First().ItemTitle.Should().NotBeNull().And.Be(_profile.CurrrentReservations.First().Copy.Item.Title);

            responseProfile.ProfileHistory.ArchivalRentals.First().ItemTitle.Should().NotBeNull().And.Be(_profile.ProfileHistory.ArchivalRentals.First().Copy.Item.Title);
            responseProfile.ProfileHistory.ArchivalReservations.First().ItemTitle.Should().NotBeNull().And.Be(_profile.ProfileHistory.ArchivalReservations.First().Copy.Item.Title);
        }

        [Fact]
        async Task GetProfileWithHistory_ForNonExistingProfile_Returns404NotFound()
        {
            _sharedContext.DbContext.Set<Profile>().Remove(_profile);
            await _sharedContext.DbContext.SaveChangesAsync();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetProfileWithHistory),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
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

            responseProfile.Should().BeEquivalentTo(_profile, options =>
                options.AllowingInfiniteRecursion()
                .ExcludingMissingMembers()
                .Excluding(profile => profile.ProfileHistory)
            );

            responseProfile.ProfileHistory.Should().BeNull();

            responseProfile.CurrrentRentals.First().ItemTitle.Should().NotBeNull().And.Be(_profile.CurrrentRentals.First().Copy.Item.Title);
            responseProfile.CurrrentReservations.First().ItemTitle.Should().NotBeNull().And.Be(_profile.CurrrentReservations.First().Copy.Item.Title);
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

        [Fact]
        async Task GetProfileWithHistoryByCardNumberAsync_ForValidCardNumber_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetProfileWithHistoryByCardNumber + $"?card-number={_profile.LibraryCardNumber}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var responseProfile = await response.Content.ReadFromJsonAsync<ProfileResponse>();

            responseProfile.Should().BeEquivalentTo(_profile, options =>
                options.AllowingInfiniteRecursion()
                .ExcludingMissingMembers()
            );

            responseProfile.CurrrentRentals.First().ItemTitle.Should().NotBeNull().And.Be(_profile.CurrrentRentals.First().Copy.Item.Title);
            responseProfile.CurrrentReservations.First().ItemTitle.Should().NotBeNull().And.Be(_profile.CurrrentReservations.First().Copy.Item.Title);

            responseProfile.ProfileHistory.ArchivalRentals.First().ItemTitle.Should().NotBeNull().And.Be(_profile.ProfileHistory.ArchivalRentals.First().Copy.Item.Title);
            responseProfile.ProfileHistory.ArchivalReservations.First().ItemTitle.Should().NotBeNull().And.Be(_profile.ProfileHistory.ArchivalReservations.First().Copy.Item.Title);
        }

        [Fact]
        async Task GetProfileWithHistoryByCardNumberAsync_ForInvalidCardNumber_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetProfileWithHistoryByCardNumber + $"?card-number=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetHistoryByCardNumberAsync_ForValidCardNumber_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetHistoryByCardNumber + $"?card-number={_profile.LibraryCardNumber}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var responseHistory = await response.Content.ReadFromJsonAsync<ProfileHistoryResponse>();

            responseHistory.Should().BeEquivalentTo(_profile.ProfileHistory, options =>
            {
                options.AllowingInfiniteRecursion();
                options.ExcludingMissingMembers();
                return options;
            });

            responseHistory.ArchivalRentals.First().ItemTitle.Should().NotBeNull().And.Be(_profile.ProfileHistory.ArchivalRentals.First().Copy.Item.Title);
            responseHistory.ArchivalReservations.First().ItemTitle.Should().NotBeNull().And.Be(_profile.ProfileHistory.ArchivalReservations.First().Copy.Item.Title);
        }

        [Fact]
        async Task GetHistoryByCardNumberAsync_ForInvalidCardNumber_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetHistoryByCardNumber + $"?card-number=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetCurrentRentalsAsync_ForValidCardNumber_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetCurrentRentals + $"?card-number={_profile.LibraryCardNumber}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var responseRentals = await response.Content.ReadFromJsonAsync<ICollection<RentalResponse>>();

            responseRentals.Should().BeEquivalentTo(_profile.CurrrentRentals, options => options.ExcludingMissingMembers());

            responseRentals.First().ItemTitle.Should().NotBeNull().And.Be(_profile.CurrrentRentals.First().Copy.Item.Title);
        }

        [Fact]
        async Task GetCurrentRentalsAsync_ForInvalidCardNumber_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetCurrentRentals + $"?card-number=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetCurrentRentalsAsync_ForEmptyCurrentRentals_ReturnsEmptyCollection()
        {
            _profile.CurrrentRentals = null;

            await _sharedContext.DbContext.SaveChangesAsync();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetCurrentRentals + $"?card-number={_profile.LibraryCardNumber}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var responseRentals = await response.Content.ReadFromJsonAsync<ICollection<RentalResponse>>();

            responseRentals.Count.Should().Be(0);
        }

        [Fact]
        async Task GetCurrentReservationsAsync_ForValidCardNumber_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetCurrentReservations + $"?card-number={_profile.LibraryCardNumber}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var responseReservations = await response.Content.ReadFromJsonAsync<ICollection<ReservationResponse>>();

            responseReservations.Should().BeEquivalentTo(_profile.CurrrentReservations, options => options.ExcludingMissingMembers());

            responseReservations.First().ItemTitle.Should().NotBeNull().And.Be(_profile.CurrrentReservations.First().Copy.Item.Title);
        }

        [Fact]
        async Task GetCurrentReservationsAsync_ForInvalidCardNumber_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetCurrentReservations + $"?card-number=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetCurrentReservationsAsync_ForEmptyCurrentRentals_ReturnsEmptyCollection()
        {
            _profile.CurrrentReservations = null;

            await _sharedContext.DbContext.SaveChangesAsync();

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_client.BaseAddress + Profiles.GetCurrentReservations + $"?card-number={_profile.LibraryCardNumber}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var responseReservations = await response.Content.ReadFromJsonAsync<ICollection<ReservationResponse>>();

            responseReservations.Count.Should().Be(0);
        }
    }
}