using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Reactive.Interfaces;
using Azure;
using CommonContext;
using CommonContext.Extensions;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net.Http.Json;
using WebAPI.ApiRoutes;
using WebAPITests.Integration.SharedContextBuilders;
using Profile = Domain.Entities.Profile;
using Reservation = Domain.Entities.Reservation;

namespace WebAPITests.Integration
{
    public class ReservationsControllerTests : IClassFixture<ReservationContextBuilder>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly Copy _copy;
        private readonly ApplicationUser _defaultUser;
        private readonly Profile _defaultProfile;
        private readonly IEnumerable<Reservation> _reservations;
        private readonly SharedContext _sharedContext;

        public ReservationsControllerTests(ReservationContextBuilder sharedContextBuilder)
        {
            _sharedContext = sharedContextBuilder.Value;
            _client = _sharedContext.CreateClient();

            _defaultUser = _sharedContext.DefaultUser;
            _defaultProfile = _sharedContext.DefaultProfile;

            _copy = DataGenerator.GetOneWithDependencies<Copy>();
            _copy.CopyHistory.Clear();

            var copy2 = DataGenerator.GetOneWithDependencies<Copy>();
            var copy3 = DataGenerator.GetOneWithDependencies<Copy>();

            copy2.IsAvailable = false;
            copy3.IsAvailable = false;
            copy2.CopyHistory.Clear();
            copy3.CopyHistory.Clear();

            _reservations = DataGenerator.Get<Reservation>(2);
            var _otherProfile = DataGenerator.GetOne<Profile>();

            _reservations.ElementAt(0).Copy = copy2;
            _reservations.ElementAt(0).Profile = _defaultProfile;

            _reservations.ElementAt(1).Copy = copy3;
            _reservations.ElementAt(1).Profile = _otherProfile;

            _sharedContext.DbContext.Set<Copy>().Add(_copy);
            _sharedContext.DbContext.Set<Reservation>().AddRange(_reservations);
            _sharedContext.DbContext.SaveChangesAsync().Wait();

            _sharedContext.RefreshDb();
        }

        [Fact]
        async Task AddReservationAsync_ForDeactiveProfile_Returns400BadRequest()
        {
            _defaultProfile.IsActive = false;
            _sharedContext.DbContext.Set<Profile>().Update(_defaultProfile);
            await _sharedContext.DbContext.SaveChangesAsync();

            var ReservationRequest = new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservation),
                Content = JsonContent.Create(ReservationRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("profile is deactivate");

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                   .Include(rProfile => rProfile.CurrentReservations)
                   .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                   .FirstOrDefaultAsync();

            refreshProfile.CurrentReservations.Count.Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationAsync_ForInvalidCopyInventoryNumber_Returns404NotFound()
        {
            var ReservationRequest = new ReservationRequest() { CopyInventoryNumber = "null_null" };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservation),
                Content = JsonContent.Create(ReservationRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("copy not found");

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentReservations.Count.Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationAsync_ForInvalidModel_Returns400BadRequest()
        {
            var ReservationRequest = new ReservationRequest() { CopyInventoryNumber = String.Empty };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservation),
                Content = JsonContent.Create(ReservationRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            validationProblemDetails.Errors.Count.Should().Be(1);

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentReservations.Count.Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationAsync_ForMaxReservations_Returns400BadRequest()
        {
            _defaultProfile.CurrentReservations = (ICollection<Reservation>)DataGenerator.Get<Reservation>(4);

            _sharedContext.DbContext.Set<Profile>().Update(_defaultProfile);
            await _sharedContext.DbContext.SaveChangesAsync();

            var ReservationRequest = new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservation),
                Content = JsonContent.Create(ReservationRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("user has a maximum number of reservations");

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(6);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                   .Include(rProfile => rProfile.CurrentReservations)
                   .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                   .FirstOrDefaultAsync();

            refreshProfile.CurrentReservations.Count().Should().Be(5);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationAsync_ForUnavailableCopy_Returns400BadRequest()
        {
            _copy.IsAvailable = false;

            _sharedContext.DbContext.Set<Copy>().Update(_copy);
            await _sharedContext.DbContext.SaveChangesAsync();

            var ReservationRequest = new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservation),
                Content = JsonContent.Create(ReservationRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("copy doesn't available");

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentReservations.Count.Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationAsync_ForValidModel_Returns201Created()
        {
            var ReservationRequest = new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservation),
                Content = JsonContent.Create(ReservationRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            var responseReservation = await response.Content.ReadFromJsonAsync<ReservationResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Reservations.Prefix}/{Reservations.GetReservationById}?id={responseReservation.Id}");

            response.Headers.Location.Should().Be(expectedLocationUri);


            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(3);

            var newReservation = await _sharedContext.DbContext.Set<Reservation>()
                .Include(Reservation => Reservation.Copy)
                .Include(Reservation => Reservation.Profile)
                .Where(Reservation => Reservation.Id == responseReservation.Id)
                .FirstOrDefaultAsync();

            newReservation.Should().NotBeNull();

            newReservation.CreatedBy.Should().Be("default");
            newReservation.LastModifiedBy.Should().Be("default");

            newReservation.Created.Should().NotBe(null).And.Be(newReservation.LastModified);

            newReservation.Copy.InventoryNumber.Should().Be(_copy.InventoryNumber);
            newReservation.Copy.IsAvailable.Should().BeFalse();
            newReservation.Profile.LibraryCardNumber.Should().Be(_defaultProfile.LibraryCardNumber);

            responseReservation.Should().BeEquivalentTo(newReservation, options => options.ExcludingMissingMembers());

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentReservations.Should().NotBeNullOrEmpty();
            refreshProfile.CurrentReservations.Count.Should().Be(2);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.Is<Reservation>(Reservation => Reservation.Copy.InventoryNumber == _copy.InventoryNumber && Reservation.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
        }

        [Fact]
        async Task AddReservationsAsync_ForDeactiveProfile_Returns400BadRequest()
        {
            _defaultProfile.IsActive = false;
            _sharedContext.DbContext.Set<Profile>().Update(_defaultProfile);
            await _sharedContext.DbContext.SaveChangesAsync();

            var ReservationsRequest = new List<ReservationRequest>() {
                new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservations),
                Content = JsonContent.Create(ReservationsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be("profile is deactivate");

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                   .Include(rProfile => rProfile.CurrentReservations)
                   .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                   .FirstOrDefaultAsync();

            refreshProfile.CurrentReservations.Should().NotBeNullOrEmpty();
            refreshProfile.CurrentReservations.Count.Should().Be(1);

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();


            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationsAsync_ForEmptyReservations_Returns400BadRequest()
        {
            var ReservationsRequest = new List<ReservationRequest>() { };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservations),
                Content = JsonContent.Create(ReservationsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            validationProblemDetails.Errors.Count().Should().Be(1);

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentReservations.Should().NotBeNullOrEmpty();
            refreshProfile.CurrentReservations.Count.Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationsAsync_ForMaxAmountReservations_Returns200Ok()
        {
            var copies = DataGenerator.GetWithDependencies<Copy>(3).ToList();
            _sharedContext.DbContext.Set<Copy>().AddRange(copies);
            await _sharedContext.DbContext.SaveChangesAsync();

            var ReservationsRequest = new List<ReservationRequest>() {
                new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new ReservationRequest() { CopyInventoryNumber = copies[0].InventoryNumber  },
                new ReservationRequest() { CopyInventoryNumber = copies[1].InventoryNumber  },
                new ReservationRequest() { CopyInventoryNumber = copies[2].InventoryNumber  }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservations),
                Content = JsonContent.Create(ReservationsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(6);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);
            var copy2 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[0].InventoryNumber);
            var copy3 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[1].InventoryNumber);

            copy1.IsAvailable.Should().BeFalse();
            copy2.IsAvailable.Should().BeFalse();
            copy3.IsAvailable.Should().BeFalse();

            refreshProfile.CurrentReservations.Should().NotBeNullOrEmpty();
            refreshProfile.CurrentReservations.Count.Should().Be(5);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.Is<Reservation>(Reservation => Reservation.Copy.InventoryNumber == _copy.InventoryNumber && Reservation.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
            endOfReservationMock.Verify(service => service.AddReservation(It.Is<Reservation>(Reservation => Reservation.Copy.InventoryNumber == copies[0].InventoryNumber && Reservation.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
            endOfReservationMock.Verify(service => service.AddReservation(It.Is<Reservation>(Reservation => Reservation.Copy.InventoryNumber == copies[1].InventoryNumber && Reservation.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Exactly(4));
        }

        [Fact]
        async Task AddReservationsAsync_ForOneCopyTwoTimes_Returns400BadRequest()
        {
            var ReservationsRequest = new List<ReservationRequest>() {
                new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber  }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservations),
                Content = JsonContent.Create(ReservationsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            validationProblemDetails.Errors.Count().Should().Be(1);

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();

            refreshProfile.CurrentReservations.Should().NotBeNullOrEmpty();
            refreshProfile.CurrentReservations.Count.Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationsAsync_ForOneInvalidCopyInventoryNumber_Returns404NotFound()
        {
            var ReservationsRequest = new List<ReservationRequest>() {
                new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new ReservationRequest() { CopyInventoryNumber = "null_null"  }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservations),
                Content = JsonContent.Create(ReservationsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var stringResponse = await response.Content.ReadAsStringAsync();

            stringResponse.Should().Be($"copy null_null not found");

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();

            refreshProfile.CurrentReservations.Should().NotBeNullOrEmpty();
            refreshProfile.CurrentReservations.Count.Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationsAsync_ForOneInvalidModel_Returns400BadRequest()
        {
            var ReservationsRequest = new List<ReservationRequest>() {
                new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new ReservationRequest() { CopyInventoryNumber = ""  }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservations),
                Content = JsonContent.Create(ReservationsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            validationProblemDetails.Errors.Count().Should().Be(1);

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();

            refreshProfile.CurrentReservations.Should().NotBeNullOrEmpty();
            refreshProfile.CurrentReservations.Count.Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationsAsync_ForOneUnavailableCopy_Returns400BadRequest()
        {
            var copies = DataGenerator.GetWithDependencies<Copy>(1).ToList();
            copies[0].IsAvailable = false;
            _sharedContext.DbContext.Set<Copy>().AddRange(copies);
            await _sharedContext.DbContext.SaveChangesAsync();

            var ReservationsRequest = new List<ReservationRequest>() {
                new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new ReservationRequest() { CopyInventoryNumber = copies[0].InventoryNumber  }};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservations),
                Content = JsonContent.Create(ReservationsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();

            responseString.Should().Be($"copy {copies[0].InventoryNumber} doesn't available");

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);
            var copy2 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[0].InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();
            copy2.IsAvailable.Should().BeFalse();

            refreshProfile.CurrentReservations.Should().NotBeNullOrEmpty();
            refreshProfile.CurrentReservations.Count.Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationsAsync_ForTooManyReservations_Returns400BadRequest()
        {
            var copies = DataGenerator.GetWithDependencies<Copy>(5).ToList();
            _sharedContext.DbContext.Set<Copy>().AddRange(copies);
            await _sharedContext.DbContext.SaveChangesAsync();

            var ReservationsRequest = new List<ReservationRequest>() {
                new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new ReservationRequest() { CopyInventoryNumber = copies[0].InventoryNumber  },
                new ReservationRequest() { CopyInventoryNumber = copies[1].InventoryNumber  },
                new ReservationRequest() { CopyInventoryNumber = copies[2].InventoryNumber  },
                new ReservationRequest() { CopyInventoryNumber = copies[3].InventoryNumber  },
                new ReservationRequest() { CopyInventoryNumber = copies[4].InventoryNumber  },};

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservations),
                Content = JsonContent.Create(ReservationsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Be("it isn't possible to add so many reservations");

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);
            var copy2 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[0].InventoryNumber);
            var copy3 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[1].InventoryNumber);
            var copy4 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[2].InventoryNumber);
            var copy5 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copies[3].InventoryNumber);

            copy1.IsAvailable.Should().BeTrue();
            copy2.IsAvailable.Should().BeTrue();
            copy3.IsAvailable.Should().BeTrue();
            copy4.IsAvailable.Should().BeTrue();
            copy5.IsAvailable.Should().BeTrue();

            refreshProfile.CurrentReservations.Should().NotBeNullOrEmpty();
            refreshProfile.CurrentReservations.Count.Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        async Task AddReservationsAsync_ForValidModels_Returns200Ok()
        {
            var copy = DataGenerator.GetWithDependencies<Copy>(1).First();
            _sharedContext.DbContext.Set<Copy>().Add(copy);
            await _sharedContext.DbContext.SaveChangesAsync();

            var ReservationsRequest = new List<ReservationRequest>() {
                new ReservationRequest() { CopyInventoryNumber = _copy.InventoryNumber },
                new ReservationRequest() { CopyInventoryNumber = copy.InventoryNumber  } };

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.AddReservations),
                Content = JsonContent.Create(ReservationsRequest)
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(4);

            var refreshedProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            var copy1 = await _sharedContext.DbContext.Set<Copy>().FindAsync(_copy.InventoryNumber);
            var copy2 = await _sharedContext.DbContext.Set<Copy>().FindAsync(copy.InventoryNumber);

            copy1.IsAvailable.Should().BeFalse();
            copy2.IsAvailable.Should().BeFalse();

            refreshedProfile.CurrentReservations.Should().NotBeNullOrEmpty();
            refreshedProfile.CurrentReservations.Count.Should().Be(3);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();

            endOfReservationMock.Verify(service => service.AddReservation(It.Is<Reservation>(Reservation => Reservation.Copy.InventoryNumber == _copy.InventoryNumber && Reservation.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
            endOfReservationMock.Verify(service => service.AddReservation(It.Is<Reservation>(Reservation => Reservation.Copy.InventoryNumber == copy.InventoryNumber && Reservation.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
        }

        [Fact]
        async Task GetReservationByIdAsync_ForInvalidId_Returns404NotFound()
        {
            var requestUri = QueryHelpers.AddQueryString(Reservations.GetReservationById, "id", "null");

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        async Task GetReservationByIdAsync_ForValidId_Returns200Ok()
        {
            var requestUri = QueryHelpers.AddQueryString(Reservations.GetReservationById, "id", _reservations.First().Id);

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ReservationResponse>();

            _sharedContext.RefreshDb();

            result.Should().BeEquivalentTo(_reservations.First(), options => options.ExcludingMissingMembers());
            result.ItemTitle.Should().NotBeNull().And.Be(_reservations.First().Copy.Item.Title);
        }

        [Fact]
        async Task GetReservationByIdAsync_ForNotItsReervation_Returns403Forbidden()
        {
            var requestUri = QueryHelpers.AddQueryString(Reservations.GetReservationById, "id", _reservations.Last().Id);

            var response = await _client.GetAsync(requestUri);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        async Task RemoveReservationByIdAsync_ForInvalidId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Reservations.RemoveReservationById + $"?id=null_null"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);
        }
        [Fact]
        async Task RemoveReservationByIdAsync_ForNotItsReervation_Returns403Forbidden()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Reservations.RemoveReservationById + $"?id={_reservations.Last().Id}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        async Task RemoveReservationByIdAsync_ForValidId_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_client.BaseAddress + Reservations.RemoveReservationById + $"?id={_reservations.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();
            endOfReservationMock.Verify(service => service.RemoveReservation(_reservations.First().Id), Times.Once);
        }

        [Fact]
        async Task RentAsync_ForValidId_Returns200Ok()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.Rent + $"?id={_reservations.First().Id}"),
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            var rentalResponse = await response.Content.ReadFromJsonAsync<RentalResponse>();
            var expectedLocationUri = new Uri($"{_sharedContext.ApplicationSettings.BaseAddress}/{_sharedContext.ApplicationSettings.RoutePrefix}/{Rentals.Prefix}/{Rentals.GetRentalById}?id={rentalResponse.Id}");

            response.Headers.Location.Should().Be(expectedLocationUri);

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(1);
            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(1);

            var newRental = await _sharedContext.DbContext.Set<Rental>()
                .Where(rental => rental.Id == rentalResponse.Id)
                .Include(rental => rental.Copy)
                .Include(rental => rental.Profile)
                .FirstOrDefaultAsync();

            newRental.Should().NotBeNull();

            newRental.CreatedBy.Should().Be("default");
            newRental.LastModifiedBy.Should().Be("default");

            newRental.Created.Should().NotBe(null).And.Be(newRental.LastModified);

            newRental.Copy.InventoryNumber.Should().Be(_reservations.First().Copy.InventoryNumber);
            newRental.Copy.IsAvailable.Should().BeFalse();
            newRental.Profile.LibraryCardNumber.Should().Be(_reservations.First().Profile.LibraryCardNumber);

            rentalResponse.Should().BeEquivalentTo(newRental, options => options.ExcludingMissingMembers());

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentReservations.Count().Should().Be(0);
            refreshProfile.CurrentRentals.Count().Should().Be(1);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();
            var countingOfPenaltyChargeMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChargeMock.Verify(service => service.AddRental(It.Is<Rental>(rental => rental.Copy.InventoryNumber == _reservations.First().Copy.InventoryNumber && rental.Profile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)), Times.Once);
            endOfReservationMock.Verify(service => service.AddReservationToHistory(It.Is<Reservation>(reservation => reservation.Id == _reservations.First().Id)), Times.Once);
        }

        [Fact]
        async Task RentAsync_ForInvalidId_Returns404NotFound()
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_client.BaseAddress + Reservations.Rent + $"?id=null_null"),
            };

            var response = await _client.SendAsync(request);

            _sharedContext.RefreshDb();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            _sharedContext.DbContext.Set<Reservation>().Count().Should().Be(2);
            _sharedContext.DbContext.Set<Rental>().Count().Should().Be(0);

            var refreshProfile = await _sharedContext.DbContext.Set<Profile>()
                    .Include(rProfile => rProfile.CurrentReservations)
                    .Include(rProfile => rProfile.CurrentRentals)
                    .Where(rProfile => rProfile.LibraryCardNumber == _defaultProfile.LibraryCardNumber)
                    .FirstOrDefaultAsync();

            refreshProfile.CurrentReservations.Count().Should().Be(1);
            refreshProfile.CurrentRentals.Count().Should().Be(0);

            var endOfReservationMock = _sharedContext.GetMock<IEndOfReservation>();
            var countingOfPenaltyChargeMock = _sharedContext.GetMock<ICountingOfPenaltyCharges>();

            countingOfPenaltyChargeMock.Verify(service => service.AddRental(It.IsAny<Rental>()), Times.Never);
            endOfReservationMock.Verify(service => service.AddReservationToHistory(It.IsAny<Reservation>()), Times.Never);
        }

        public void Dispose()
        {
            _sharedContext.ResetState();
        }
    }
}